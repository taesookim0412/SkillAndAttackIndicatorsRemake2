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
    public class PortalBuilder : AbstractAbilityFXBuilder
    {
        private static readonly int PortalStateLength = Enum.GetNames(typeof(PortalState)).Length;

        [NonSerialized]
        public PlayerClientData PlayerClientData;
        [NonSerialized]
        public PlayerComponent PlayerTransparentClone;
        [NonSerialized]
        public PortalOrbClear PortalOrb;
        [NonSerialized]
        public CrimsonAuraBlack CrimsonAura;

        [Range(0f, 2f), SerializeField]
        private float PortalScaleDuration;
        [Range(0f, 2f), SerializeField]
        private float PlayerOpacityDuration;
        [Range(0f, 2f), SerializeField]
        private float PlayerOpaqueDuration;
        [SerializeField]
        private Vector3 PortalScaleMin;
        [SerializeField]
        private Vector3 PortalScaleMax;
        [SerializeField]
        public Vector3 PortalOrbOffsetPosition;
        [SerializeField]
        private Vector3 CrimsonAuraOffsetPosition;
        [SerializeField]
        public bool IsTeleportSource;

        // incompatible with onvalidate
        [HideInInspector]
        private float RequiredDurationMult;

        [HideInInspector]
        private Vector3 PortalScaleDifference;

        public override void ManualAwake()
        {
            PortalScaleDifference = PortalScaleMax - PortalScaleMin;
        }
        private float GetRequiredDurationMillis()
        {
            return (PortalScaleDuration + PlayerOpacityDuration + PlayerOpaqueDuration) * 1000f +
                (SkillAndAttackIndicatorSystem.FixedTimestep * PortalStateLength * 2f);
        }
        private void ResetRequiredDuration()
        {
            PortalScaleTimer.RequiredDuration = (long)(PortalScaleDuration * 1000f);
            PlayerOpacityTimer.RequiredDuration = (long)(PlayerOpacityDuration * 1000f);
            PlayerOpaqueTimer.RequiredDuration = (long)(PlayerOpaqueDuration * 1000f);
        }

        private void OnValidate()
        {
            ResetRequiredDuration();

            if (PortalOrb != null)
            {
                PortalOrb.transform.localPosition = PortalOrbOffsetPosition;
            }
            if (CrimsonAura != null)
            {
                CrimsonAura.transform.localPosition = CrimsonAuraOffsetPosition;
            }

            ManualAwake();
        }

        [HideInInspector]
        private PortalState PortalState; 
        [HideInInspector]
        private TimerStructDco_Observer PortalScaleTimer;
        [HideInInspector]
        private TimerStructDco_Observer PlayerOpacityTimer;
        [HideInInspector]
        private TimerStructDco_Observer PlayerOpaqueTimer;
        [HideInInspector]
        private bool RequiredDurationsModified = false;

        //public string DebugLogRequiredDurations()
        //{
        //    return $"{PortalScaleTimer.RequiredDuration}, {PlayerOpacityTimer.RequiredDuration}, {PlayerOpaqueTimer.RequiredDuration}";
        //}
        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PlayerComponent playerTransparentClone,
            PortalOrbClear portalOrb, CrimsonAuraBlack crimsonAura, long? durationAllowed)
        {
            base.Initialize(observerUpdateCache);
            
            if (durationAllowed != null)
            {
                float requiredDurationMultTimes1000 = (long) durationAllowed / GetRequiredDurationMillis() * 1000f;
                PortalScaleTimer.RequiredDuration = (long)(PortalScaleDuration * requiredDurationMultTimes1000);
                PlayerOpacityTimer.RequiredDuration = (long)(PlayerOpacityDuration * requiredDurationMultTimes1000);
                PlayerOpaqueTimer.RequiredDuration = (long)(PlayerOpaqueDuration * requiredDurationMultTimes1000);
                RequiredDurationsModified = true;
            }
            else
            {
                if (RequiredDurationsModified)
                {
                    ResetRequiredDuration();
                    RequiredDurationsModified = false;
                }
            }

            PortalScaleTimer.ObserverUpdateCache = observerUpdateCache;
            PlayerOpacityTimer.ObserverUpdateCache = observerUpdateCache;
            PlayerOpaqueTimer.ObserverUpdateCache = observerUpdateCache;

            PlayerClientData = playerClientData;

            PlayerTransparentClone = playerTransparentClone;

            PlayerComponent playerComponent = playerClientData.PlayerComponent;

            if (!IsTeleportSource)
            {
                playerComponent.gameObject.SetActive(false);
            }

            PortalOrb = portalOrb;
            portalOrb.DisableSystems();
            
            CrimsonAura = crimsonAura;
            crimsonAura.DisableSystems();

            PortalState = PortalState.PortalCreate;
        }
        public void ManualUpdate()
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

                    PortalScaleTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                    PortalState = PortalState.PortalScale;
                    break;
                case PortalState.PortalScale:
                    if (PortalScaleTimer.IsTimeNotElapsed_FixedUpdateThread())
                    {
                        float scalePercentage = PortalScaleTimer.RemainingDurationPercentage();
                        Vector3 scaleAddend = PortalScaleDifference * scalePercentage;
                        PortalOrb.transform.localScale = PortalScaleMin + scaleAddend;
                    }
                    else
                    {
                        PortalOrb.transform.localScale = PortalScaleMax;
                        PortalState = PortalState.PlayerCreate;
                    }
                    break;
                case PortalState.PlayerCreate:
                    PlayerTransparentClone.gameObject.SetActive(true);
                    PlayerTransparentClone.transform.position = transform.position;
                    PlayerClientData.PlayerComponent.transform.position = transform.position;
                    float playerTransparentCloneOpacity;
                    if (IsTeleportSource)
                    {
                        PlayerClientData.PlayerComponent.gameObject.SetActive(false);
                        playerTransparentCloneOpacity = 1f;
                    }
                    else
                    {
                        playerTransparentCloneOpacity = 0f;
                    }

                    PlayerTransparentClone.SetCloneFXOpacity(playerTransparentCloneOpacity);

                    PlayerOpacityTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                    PortalState = PortalState.PlayerOpaque;
                    break;
                case PortalState.PlayerOpaque:
                    if (PlayerOpacityTimer.IsTimeNotElapsed_FixedUpdateThread())
                    {
                        float scalePercentage = PlayerOpacityTimer.RemainingDurationPercentage();
                        if (IsTeleportSource)
                        {
                            scalePercentage = 1f - scalePercentage;
                        }
                        PlayerTransparentClone.SetCloneFXOpacity(scalePercentage);
                    }
                    else
                    {
                        PlayerTransparentClone.gameObject.SetActive(false);
                        PlayerClientData.PlayerComponent.gameObject.SetActive(!IsTeleportSource);
                        
                        PlayerOpaqueTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                        PortalState = PortalState.PlayerDespawn;
                    }
                    break;
                case PortalState.PlayerDespawn:
                    if (PlayerOpaqueTimer.IsTimeElapsed_FixedUpdateThread())
                    {
                        Complete();
                    }
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
            ObserverUpdateCache = null;
            PlayerClientData = null;
            PlayerTransparentClone = null;
            PortalOrb = null;
            CrimsonAura = null;
        }
    }
    public enum PortalState
    {
        PortalCreate,
        PortalScale,
        PlayerCreate,
        PlayerOpaque,
        PlayerDespawn
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
