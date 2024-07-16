using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class PortalBuilder : AbstractAbilityFXBuilder
    {
        [NonSerialized]
        public ObserverUpdateCache ObserverUpdateCache;
        [NonSerialized]
        public PlayerClientData PlayerClientData;
        [NonSerialized]
        public PortalOrbPurple PortalOrb;
        [NonSerialized]
        public CrimsonAuraBlack CrimsonAura;

        [Range(0f, 2f), SerializeField]
        public float PortalScaleDuration;
        [Range(0f, 2f), SerializeField]
        public float PlayerOpacityDuration;
        [Range(0f, 2f), SerializeField]
        public float PlayerOpaqueDuration;
        [SerializeField]
        public Vector3 PortalScaleMin;
        [SerializeField]
        public Vector3 PortalScaleMax;
        [SerializeField]
        public Vector3 PortalOrbOffsetPosition;
        [SerializeField]
        public Vector3 CrimsonAuraOffsetPosition;
        [SerializeField]
        public bool SetPlayerInactive;
        [SerializeField]
        public bool IsTeleportSource;

        [HideInInspector]
        private Vector3 PortalScaleDifference;

        public void Awake()
        {
            PortalScaleDifference = PortalScaleMax - PortalScaleMin;
        }

        private void OnValidate()
        {
            PortalScaleTimer.RequiredDuration = (long) (PortalScaleDuration * 1000f);
            PlayerOpacityTimer.RequiredDuration = (long)(PlayerOpacityDuration * 1000f);
            PlayerOpaqueTimer.RequiredDuration = (long)(PlayerOpaqueDuration * 1000f);

            PortalOrb.transform.localPosition = PortalOrbOffsetPosition;
            CrimsonAura.transform.localPosition = CrimsonAuraOffsetPosition;

            Awake();
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
        public bool Active = false;
        [HideInInspector]
        public bool Completed = false;

        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PortalOrbPurple portalOrb, CrimsonAuraBlack crimsonAura)
        {
            ObserverUpdateCache = observerUpdateCache;
            PlayerClientData = playerClientData;

            PlayerComponent playerComponent = playerClientData.PlayerComponent;
            playerComponent.transform.localPosition = Vector3.zero;
            playerComponent.transform.SetParent(transform, worldPositionStays: false);
            if (!IsTeleportSource)
            {
                playerComponent.gameObject.SetActive(false);
            }

            PortalOrb = portalOrb;
            portalOrb.transform.localPosition = PortalOrbOffsetPosition;
            portalOrb.transform.SetParent(transform, worldPositionStays: false);
            portalOrb.DisableParticleSystems();
            
            CrimsonAura = crimsonAura;
            crimsonAura.transform.localPosition = CrimsonAuraOffsetPosition;
            crimsonAura.transform.SetParent(transform, worldPositionStays: false);
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
                    PortalOrb.transform.localScale = Vector3.zero;
                    PortalOrb.EnableParticleSystems();
                    CrimsonAura.EnableParticleSystems();
                    PortalScaleTimer = new TimerStructDco_Observer(ObserverUpdateCache, ObserverUpdateCache.UpdateTickTimeFixedUpdate, (long)(PortalScaleDuration * 1000f));
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
                    }
                    PlayerOpacityTimer = new TimerStructDco_Observer(ObserverUpdateCache, ObserverUpdateCache.UpdateTickTimeFixedUpdate, (long)(PlayerOpacityDuration * 1000f));
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
                            PlayerOpaqueTimer = new TimerStructDco_Observer(ObserverUpdateCache, ObserverUpdateCache.UpdateTickTimeFixedUpdate, (long)(PlayerOpaqueDuration * 1000f));
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

        public void ManualDisable()
        {
            ObserverUpdateCache = null;
            PlayerClientData = null;
            PortalOrb = null;
            CrimsonAura = null;
        }

        public void EditorDestroy()
        {
            ObserverUpdateCache = null;
            GameObject.DestroyImmediate(PlayerClientData.PlayerComponent.gameObject);
            GameObject.DestroyImmediate(CrimsonAura.gameObject);
            GameObject.DestroyImmediate(PortalOrb.gameObject);

            PlayerClientData = null;
            PortalOrb = null;
            CrimsonAura = null;
        }
    }
    enum PortalState
    {
        PortalCreate,
        PortalScale,
        PlayerCreate,
        PlayerOpaque,
        PlayerDespawn
    }

    [CustomEditor(typeof(PortalBuilder))]
    public class PortalBuilderEditor : AbstractEditor
    {
        [HideInInspector]
        private PortalBuilder Instance;

        private bool VariablesSet = false;
        private bool VariablesAdded = false;
        private bool SkipDestroy = false;
        private long PreviousSimulationTime = 0;
        private int Iteration;
        public void OnSceneGUI()
        {
            Initialize();

            if (VariablesSet)
            {
                long previousUpdateTickTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                ObserverUpdateCache.Update_FixedUpdate();
                Instance.ManualUpdate();
            }
        }
        public void OnDisable()
        {
            if (!SkipDestroy && VariablesAdded)
            {
                Instance.EditorDestroy();
                VariablesSet = false;
                VariablesAdded = false;
            }
        }

        private void Initialize()
        {
            if (!VariablesSet)
            {
                VariablesSet = Instance != null && Instance.PlayerClientData != null && Instance.PortalOrb != null && Instance.CrimsonAura != null;
                if (VariablesSet)
                {
                    ObserverUpdateCache = Instance.ObserverUpdateCache;
                }
            }
            if (!VariablesSet)
            {
                SkillAndAttackIndicatorSystem instance = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
                if (instance != null)
                {
                    PlayerComponent playerComponentPrefab = instance.PlayerComponent;

                    string portalOrbPurpleType = AbilityFXComponentType.PortalOrbPurple.ToString();
                    PortalOrbPurple portalOrbPrefab = (PortalOrbPurple)instance.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == portalOrbPurpleType);

                    string crimsonAuraBlackType = AbilityFXComponentType.CrimsonAuraBlack.ToString();
                    CrimsonAuraBlack crimsonAuraPrefab = (CrimsonAuraBlack)instance.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == crimsonAuraBlackType);

                    if (playerComponentPrefab != null && portalOrbPrefab != null && crimsonAuraPrefab != null)
                    {
                        PlayerComponent playerComponent = playerComponentPrefab.CreateInactiveTransparentCloneInstance();
                        playerComponent.transform.SetParent(Instance.transform, false);
                        if (Instance.IsTeleportSource)
                        {
                            playerComponent.gameObject.SetActive(true);
                        }
                        PlayerClientData playerClientData = new PlayerClientData(playerComponent);

                        PortalOrbPurple portalOrb = GameObject.Instantiate(portalOrbPrefab, Instance.transform);

                        CrimsonAuraBlack crimsonAura = GameObject.Instantiate(crimsonAuraPrefab, Instance.transform);

                        SetObserverUpdateCache();
                        Instance.Awake();
                        Instance.Initialize(ObserverUpdateCache, playerClientData, portalOrb, crimsonAura);
                        TryAddNonPrefabParticleSystem(Instance.gameObject);
                        VariablesAdded = true;
                        VariablesSet = true;
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
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Instance = (PortalBuilder)target;

            Undo.RecordObject(Instance, "Editor State");

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

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

            SkipDestroy = GUILayout.Toggle(SkipDestroy, "SkipDestroy");
        }
    }
}
