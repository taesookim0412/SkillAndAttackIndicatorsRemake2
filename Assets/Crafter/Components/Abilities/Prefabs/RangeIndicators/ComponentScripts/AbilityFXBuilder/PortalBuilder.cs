﻿using Assets.Crafter.Components.Editors.ComponentScripts;
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

        [HideInInspector]
        private Vector3 PortalScaleDifference;

        private void OnValidate()
        {
            PortalScaleTimer.RequiredDuration = (long) (PortalScaleDuration * 1000f);
            PlayerOpacityTimer.RequiredDuration = (long)(PlayerOpacityDuration * 1000f);
            PlayerOpaqueTimer.RequiredDuration = (long)(PlayerOpaqueDuration * 1000f);

            PortalOrb.transform.localPosition = PortalOrbOffsetPosition;
            CrimsonAura.transform.localPosition = CrimsonAuraOffsetPosition;

            PortalScaleDifference = PortalScaleMax - PortalScaleMin;
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
        private bool Completed = false;

        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PortalOrbPurple portalOrb, CrimsonAuraBlack crimsonAura, bool setPlayerInactive)
        {
            ObserverUpdateCache = observerUpdateCache;
            PlayerClientData = playerClientData;

            PlayerComponent playerComponent = playerClientData.PlayerComponent;
            playerComponent.gameObject.SetActive(false);
            playerComponent.transform.localPosition = Vector3.zero;

            PortalOrb = portalOrb;
            portalOrb.DisableParticleSystems();
            portalOrb.transform.localPosition = PortalOrbOffsetPosition;
            

            CrimsonAura = crimsonAura;
            crimsonAura.DisableParticleSystems();
            crimsonAura.transform.localPosition = CrimsonAuraOffsetPosition;

            SetPlayerInactive = setPlayerInactive;
            PortalState = PortalState.PortalCreate;

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
                    PlayerClientData.PlayerComponent.gameObject.SetActive(true);
                    PlayerOpacityTimer = new TimerStructDco_Observer(ObserverUpdateCache, ObserverUpdateCache.UpdateTickTimeFixedUpdate, (long)(PlayerOpacityDuration * 1000f));
                    PortalState = PortalState.PlayerOpaque;
                    break;
                case PortalState.PlayerOpaque:
                    if (PlayerOpacityTimer.IsTimeNotElapsed_FixedUpdateThread())
                    {
                        float scalePercentage = PlayerOpacityTimer.RemainingDurationPercentage();
                        PlayerClientData.PlayerComponent.SetCloneFXOpacity(scalePercentage);
                    }
                    else
                    {
                        PlayerClientData.PlayerComponent.SetCloneFXOpacity(1f);

                        if (SetPlayerInactive)
                        {
                            PlayerOpaqueTimer = new TimerStructDco_Observer(ObserverUpdateCache, ObserverUpdateCache.UpdateTickTimeFixedUpdate, (long)(PlayerOpaqueDuration * 1000f));
                            PlayerOpaqueTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                        }
                        PortalOrb.gameObject.SetActive(false);
                        PortalState = PortalState.PlayerDespawn;
                    }
                    break;
                case PortalState.PlayerDespawn:
                    if (SetPlayerInactive)
                    {
                        if (PlayerOpaqueTimer.IsTimeElapsed_FixedUpdateThread())
                        {
                            CrimsonAura.gameObject.SetActive(false);
                            PlayerClientData.PlayerComponent.gameObject.SetActive(false);
                            Completed = true;
                        }
                    }
                    else
                    {
                        CrimsonAura.gameObject.SetActive(false);
                        Completed = true;
                    }
                    break;
            }
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
                        PlayerClientData playerClientData = new PlayerClientData(playerComponent);

                        PortalOrbPurple portalOrb = GameObject.Instantiate(portalOrbPrefab, Instance.transform);

                        CrimsonAuraBlack crimsonAura = GameObject.Instantiate(crimsonAuraPrefab, Instance.transform);

                        SetObserverUpdateCache();
                        Instance.Initialize(ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, Instance.SetPlayerInactive);
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
