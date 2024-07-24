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
        public ObserverUpdateCache ObserverUpdateCache;
        [NonSerialized]
        public PlayerClientData PlayerClientData;
        [NonSerialized]
        public PortalOrbPurple PortalOrb;
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
        private Vector3 PortalOrbOffsetPosition;
        [SerializeField]
        private Vector3 CrimsonAuraOffsetPosition;
        [SerializeField]
        public bool IsTeleportSource;
        [SerializeField]
        public bool SetPlayerInactive;
        [HideInInspector]
        private bool IsClone;

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
        [HideInInspector]
        public bool Active = false;
        [HideInInspector]
        public bool Completed = false;

        //public string DebugLogRequiredDurations()
        //{
        //    return $"{PortalScaleTimer.RequiredDuration}, {PlayerOpacityTimer.RequiredDuration}, {PlayerOpaqueTimer.RequiredDuration}";
        //}
        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PortalOrbPurple portalOrb, CrimsonAuraBlack crimsonAura, long? durationAllowed,
            bool setPlayerInactive, bool isClone)
        {
            InitializeManualAwake();
            
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

            SetPlayerInactive = setPlayerInactive;

            PortalScaleTimer.ObserverUpdateCache = observerUpdateCache;
            PlayerOpacityTimer.ObserverUpdateCache = observerUpdateCache;
            PlayerOpaqueTimer.ObserverUpdateCache = observerUpdateCache;

            ObserverUpdateCache = observerUpdateCache;
            PlayerClientData = playerClientData;

            PlayerComponent playerComponent = playerClientData.PlayerComponent;
            if (isClone)
            {
                playerComponent.transform.localPosition = Vector3.zero;
                playerComponent.transform.SetParent(transform, worldPositionStays: false);
            }

            IsClone = isClone;


            if (!IsTeleportSource)
            {
                playerComponent.gameObject.SetActive(false);
            }

            PortalOrb = portalOrb;
            portalOrb.DisableParticleSystems();
            
            CrimsonAura = crimsonAura;
            crimsonAura.DisableParticleSystems();
            
            PortalState = PortalState.PortalCreate;

            Active = false;
            Completed = false;
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
                    Transform portalOrbTransform = PortalOrb.transform;
                    portalOrbTransform.localPosition = PortalOrbOffsetPosition;
                    portalOrbTransform.SetParent(transform, worldPositionStays: false);
                    portalOrbTransform.localScale = Vector3.zero;
                    PortalOrb.EnableParticleSystems();

                    Transform crimsonAuraTransform = CrimsonAura.transform;
                    crimsonAuraTransform.localPosition = CrimsonAuraOffsetPosition;
                    crimsonAuraTransform.SetParent(transform, worldPositionStays: false);
                    CrimsonAura.EnableParticleSystems();

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
                        PortalState = PortalState.PlayerCreate;
                    }
                    break;
                case PortalState.PlayerCreate:
                    if (!IsTeleportSource)
                    {
                        PlayerClientData.PlayerComponent.gameObject.SetActive(true);
                        if (!IsClone)
                        {
                            PlayerClientData.PlayerComponent.transform.position = transform.position;
                        }
                    }
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
                        PlayerClientData.PlayerComponent.SetCloneFXOpacity(scalePercentage);
                    }
                    else
                    {
                        PlayerClientData.PlayerComponent.SetCloneFXOpacity(IsTeleportSource ? 0f : 1f);

                        if (IsTeleportSource || SetPlayerInactive)
                        {
                            PlayerOpaqueTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                        }
                        PortalOrb.DisableParticleSystems();
                        PortalState = PortalState.PlayerDespawn;
                    }
                    break;
                case PortalState.PlayerDespawn:
                    if (IsTeleportSource || SetPlayerInactive)
                    {
                        if (PlayerOpaqueTimer.IsTimeElapsed_FixedUpdateThread())
                        {
                            CrimsonAura.DisableParticleSystems();
                            PlayerClientData.PlayerComponent.gameObject.SetActive(false);
                            Completed = true;
                        }
                    }
                    else
                    {
                        CrimsonAura.DisableParticleSystems();
                        Completed = true;
                    }
                    break;
            }
        }
        public void Complete()
        {
            //PortalOrb.gameObject.SetActive(false);
            //CrimsonAura.gameObject.SetActive(false);

            //if (IsTeleportSource)
            //{
            //    PlayerClientData.PlayerComponent.gameObject.SetActive(false);
            //}
            PortalOrb.DisableParticleSystems();
            CrimsonAura.DisableParticleSystems();
            Completed = true;
        }

        public override void CleanUpInstance()
        {
            ObserverUpdateCache = null;
            PlayerClientData = null;
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
        protected override bool OnInitialize(PortalBuilder instance)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {
                PlayerComponent playerComponentPrefab = system.PlayerComponent;

                string portalOrbPurpleType = AbilityFXComponentType.PortalOrbPurple.ToString();
                PortalOrbPurple portalOrbPrefab = (PortalOrbPurple)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == portalOrbPurpleType);

                string crimsonAuraBlackType = AbilityFXComponentType.CrimsonAuraBlack.ToString();
                CrimsonAuraBlack crimsonAuraPrefab = (CrimsonAuraBlack)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == crimsonAuraBlackType);

                if (playerComponentPrefab != null && portalOrbPrefab != null && crimsonAuraPrefab != null)
                {
                    PlayerComponent playerComponent = playerComponentPrefab.CreateInactiveTransparentCloneInstance();
                    playerComponent.transform.SetParent(instance.transform, false);
                    if (instance.IsTeleportSource)
                    {
                        playerComponent.gameObject.SetActive(true);
                    }
                    PlayerClientData playerClientData = new PlayerClientData(playerComponent);

                    PortalOrbPurple portalOrb = GameObject.Instantiate(portalOrbPrefab, instance.transform);

                    CrimsonAuraBlack crimsonAura = GameObject.Instantiate(crimsonAuraPrefab, instance.transform);

                    SetObserverUpdateCache();
                    instance.ManualAwake();
                    instance.Initialize(ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, null, instance.SetPlayerInactive, isClone: true);
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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            PlayerComponent playerComponent = (PlayerComponent) EditorGUILayout.ObjectField("PlayerComponent", Instance.PlayerClientData != null ? Instance.PlayerClientData.PlayerComponent : null, typeof(PlayerComponent), true);
            PortalOrbPurple portalOrbPurple = (PortalOrbPurple) EditorGUILayout.ObjectField("PortalOrb", Instance.PortalOrb, typeof(PortalOrbPurple), true);
            CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack) EditorGUILayout.ObjectField("CrimsonAura", Instance.CrimsonAura, typeof(CrimsonAuraBlack), true);

            bool restartButton = GUILayout.Button("Restart");
            if (EditorGUI.EndChangeCheck() || restartButton) {
                OnDisable();
                ParticleSystem particleSystem = Instance.GetComponent<ParticleSystem>();
                GameObject.DestroyImmediate(particleSystem);
            }

        }

        protected override void ManualUpdate()
        {
            Instance.ManualUpdate();
        }

        protected override void EditorDestroy()
        {
            Instance.ObserverUpdateCache = null;
            GameObject.DestroyImmediate(Instance.PlayerClientData.PlayerComponent.gameObject);
            GameObject.DestroyImmediate(Instance.CrimsonAura.gameObject);
            GameObject.DestroyImmediate(Instance.PortalOrb.gameObject);

            Instance.PlayerClientData = null;
            Instance.PortalOrb = null;
            Instance.CrimsonAura = null;
        }
    }
}
