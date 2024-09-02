using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFX;
using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class PortalBuilder : PlayerBlinkBuilder
    {
        private static readonly int PortalStateLength = Enum.GetNames(typeof(PortalState)).Length;

        [NonSerialized]
        public PortalOrbClear PortalOrb;
        [NonSerialized]
        public CrimsonAuraBlack CrimsonAura;

        [Range(0f, 2f), SerializeField]
        private float PortalScaleDuration;
        [SerializeField]
        private Vector3 PortalScaleMin;
        [SerializeField]
        private Vector3 PortalScaleMax;
        [SerializeField]
        public Vector3 PortalOrbOffsetPosition;
        [SerializeField]
        private Vector3 CrimsonAuraOffsetPosition;

        // incompatible with onvalidate
        [NonSerialized, HideInInspector]
        private float RequiredDurationMult;

        [NonSerialized, HideInInspector]
        private Vector3 PortalScaleDifference;

        public override void ManualAwake()
        {
            PortalScaleDifference = PortalScaleMax - PortalScaleMin;
        }
        protected override float GetRequiredDurationMillis(ObserverUpdateCache observerUpdateCache)
        {
            return PortalScaleDuration * 1000f +
                (observerUpdateCache.UpdateRenderThreadAverageTimeStep * PortalStateLength * 2f) + base.GetRequiredDurationMillis(observerUpdateCache);
        }
        protected override void ResetRequiredDuration()
        {
            PortalScaleTimer.RequiredDuration = (long)(PortalScaleDuration * 1000f);
            base.ResetRequiredDuration();
        }
        protected override void UpdateOnValidatePositions()
        {
            if (PortalOrb != null)
            {
                PortalOrb.transform.localPosition = PortalOrbOffsetPosition;
            }
            if (CrimsonAura != null)
            {
                CrimsonAura.transform.localPosition = CrimsonAuraOffsetPosition;
            }
        }

        [NonSerialized, HideInInspector]
        private PortalState PortalState; 
        [NonSerialized, HideInInspector]
        private TimerStructDco_Observer PortalScaleTimer;

        //public string DebugLogRequiredDurations()
        //{
        //    return $"{PortalScaleTimer.RequiredDuration}, {PlayerOpacityTimer.RequiredDuration}, {PlayerOpaqueTimer.RequiredDuration}";
        //}
        protected override void InitializeDurations(float requiredDurationMultTimes1000)
        {
            PortalScaleTimer.RequiredDuration = (long)(PortalScaleDuration * requiredDurationMultTimes1000);
            base.InitializeDurations(requiredDurationMultTimes1000);
        }
        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PlayerComponent playerTransparentClone,
            PortalOrbClear portalOrb, CrimsonAuraBlack crimsonAura, long? durationAllowed)
        {
            base.Initialize(observerUpdateCache, playerClientData, playerTransparentClone, durationAllowed);
            
            PortalScaleTimer.ObserverUpdateCache = observerUpdateCache;

            PortalOrb = portalOrb;
            portalOrb.DisableSystems();
            
            CrimsonAura = crimsonAura;
            crimsonAura.DisableSystems();

            PortalState = PortalState.PortalCreate;
        }
        public override void ManualUpdate()
        {
            if (Completed)
            {
                return;
            }
            switch (PortalState)
            {
                case PortalState.PortalCreate:
                    Active = true;

                    Vector3 playerPosition = transform.position;
                    Transform portalOrbTransform = PortalOrb.transform;
                    portalOrbTransform.position = playerPosition + PortalOrbOffsetPosition;
                    portalOrbTransform.localScale = PortalScaleMin;
                    PortalOrb.EnableSystems();

                    CrimsonAura.transform.position = playerPosition + CrimsonAuraOffsetPosition;
                    CrimsonAura.EnableSystems();

                    PortalScaleTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeRenderThread;
                    PortalState = PortalState.PortalScale;
                    break;
                case PortalState.PortalScale:
                    if (PortalScaleTimer.IsTimeNotElapsed_RenderThread())
                    {
                        float scalePercentage = PortalScaleTimer.RemainingDurationPercentage_RenderThread();
                        Vector3 scaleAddend = PortalScaleDifference * scalePercentage;
                        PortalOrb.transform.localScale = PortalScaleMin + scaleAddend;
                    }
                    else
                    {
                        PortalOrb.transform.localScale = PortalScaleMax;
                        PortalState = PortalState.PlayerBlinkState;
                    }
                    break;
                case PortalState.PlayerBlinkState:
                    base.ManualUpdate();
                    break;
            }
        }
        public override void Complete()
        {
            PortalOrb.DisableSystems();
            CrimsonAura.DisableSystems();
            base.Complete();
        }

        public override void CleanUpInstance()
        {
            PortalOrb = null;
            CrimsonAura = null;
        }
    }
    public enum PortalState
    {
        PortalCreate,
        PortalScale,
        PlayerBlinkState
    }

    [CustomEditor(typeof(PortalBuilder))]
    public class PortalBuilderEditor : AbstractEditor<PortalBuilder>
    {
        public long? RequiredDuration = null;
        protected override bool OnInitialize(PortalBuilder instance, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {
                PlayerComponent playerComponentPrefab = system.PlayerComponent;

                string portalOrbType = AbilityFXComponentType.PortalOrbClear.ToString();
                PortalOrbClear portalOrbPrefab = (PortalOrbClear)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == portalOrbType);

                string crimsonAuraBlackType = AbilityFXComponentType.CrimsonAuraBlack.ToString();
                CrimsonAuraBlack crimsonAuraPrefab = (CrimsonAuraBlack)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == crimsonAuraBlackType);

                if (playerComponentPrefab != null && portalOrbPrefab != null && crimsonAuraPrefab != null)
                {
                    // for lack of better way to create the same player component, just use a transparent clone for the editor.
                    PlayerComponent playerComponentInstance = playerComponentPrefab.CreateInactiveTransparentCloneInstance();
                    playerComponentInstance.transform.SetParent(instance.transform, false);
                    PlayerComponent playerTransparentClone = playerComponentPrefab.CreateInactiveTransparentCloneInstance();
                    playerTransparentClone.transform.SetParent(instance.transform, false);
                    if (instance.IsTeleportSource)
                    {
                        playerComponentInstance.gameObject.SetActive(true);
                    }
                    PlayerClientData playerClientData = new PlayerClientData(system.PlayerGuid, playerComponentInstance);

                    PortalOrbClear portalOrb = GameObject.Instantiate(portalOrbPrefab, instance.transform);

                    CrimsonAuraBlack crimsonAura = GameObject.Instantiate(crimsonAuraPrefab, instance.transform);

                    if (observerUpdateCache == null)
                    {
                        SetObserverUpdateCache();
                        observerUpdateCache = ObserverUpdateCache;
                    }
                    
                    instance.Initialize(observerUpdateCache, playerClientData, playerTransparentClone, portalOrb, crimsonAura, RequiredDuration);
                    TryAddParticleSystem(instance.gameObject);
                    return true;
                }
                else
                {
                    Debug.LogError("Couldn't find FX.");
                }
            }
            else
            {
                Debug.LogError("System null");
            }
            return false;
        }
        //private void Initialize()
        //{
        //    //if (!VariablesSet)
        //    //{
        //    //    Instance = (PortalBuilder) target;
        //    //    VariablesSet = Instance != null && Instance.PlayerClientData != null && Instance.PortalOrb != null && Instance.CrimsonAura != null;
        //    //    if (VariablesSet)
        //    //    {
        //    //        ObserverUpdateCache = Instance.ObserverUpdateCache;
        //    //    }
        //    //}

        //}

        //public override void OnInspectorGUI()
        //{
        //    base.OnInspectorGUI();

        //    EditorGUI.BeginChangeCheck();
        //    PlayerComponent playerComponent = (PlayerComponent) EditorGUILayout.ObjectField("PlayerComponent", Instance.PlayerClientData != null ? Instance.PlayerClientData.PlayerComponent : null, typeof(PlayerComponent), true);
        //    PortalOrbPurple portalOrbPurple = (PortalOrbPurple) EditorGUILayout.ObjectField("PortalOrb", Instance.PortalOrb, typeof(PortalOrbPurple), true);
        //    CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack) EditorGUILayout.ObjectField("CrimsonAura", Instance.CrimsonAura, typeof(CrimsonAuraBlack), true);
        //}

        protected override void ManualUpdate()
        {
            Instance.ManualUpdate();
        }

        protected override void EditorDestroy()
        {
            GameObject.DestroyImmediate(Instance.PlayerClientData.PlayerComponent.gameObject);
            GameObject.DestroyImmediate(Instance.PlayerTransparentClone.gameObject);
            GameObject.DestroyImmediate(Instance.CrimsonAura.gameObject);
            GameObject.DestroyImmediate(Instance.PortalOrb.gameObject);

            Instance.CleanUpInstance();
        }
    }
}
