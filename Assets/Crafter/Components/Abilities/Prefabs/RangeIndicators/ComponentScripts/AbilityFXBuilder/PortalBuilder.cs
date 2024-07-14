using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public bool SetPlayerInactive;

        private void OnValidate()
        {
            PortalScaleTimer.RequiredDuration = (long) (PortalScaleDuration * 1000f);
            PlayerOpacityTimer.RequiredDuration = (long)(PlayerOpacityDuration * 1000f);
            PlayerOpaqueTimer.RequiredDuration = (long)(PlayerOpaqueDuration * 1000f);
        }

        [HideInInspector]
        private TimerStructDco_Observer PortalScaleTimer;
        [HideInInspector]
        private TimerStructDco_Observer PlayerOpacityTimer;
        [HideInInspector]
        private TimerStructDco_Observer PlayerOpaqueTimer;
        [HideInInspector]
        private bool PortalCreated;
        [HideInInspector]
        private bool PortalScaleCompleted;
        [HideInInspector]
        private bool PlayerCreated;
        [HideInInspector]
        private bool PlayerOpaque;
        [HideInInspector]
        private bool PlayerDespawned;
        [HideInInspector]
        private bool Completed = false;

        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PortalOrbPurple portalOrb, CrimsonAuraBlack crimsonAura, bool setPlayerInactive)
        {
            ObserverUpdateCache = observerUpdateCache;
            PlayerClientData = playerClientData;
            PortalOrb = portalOrb;
            CrimsonAura = crimsonAura;
            SetPlayerInactive = setPlayerInactive;
            PortalCreated = false;
            PortalScaleCompleted = false;
            PlayerCreated = false;
            PlayerOpaque = false;
            PlayerDespawned = false;
            Completed = false;
        }
        public void ManualUpdate()
        {
            if (Completed)
            {
                return;
            }
            if (!PortalCreated)
            {
                PortalOrb.transform.localScale = Vector3.zero;
                PortalOrb.gameObject.SetActive(true);
                PortalScaleTimer = new TimerStructDco_Observer(ObserverUpdateCache, ObserverUpdateCache.UpdateTickTimeFixedUpdate,(long)(PortalScaleDuration * 1000f));
                PortalCreated = true;
            }
            else if (!PortalScaleCompleted)
            {
                if (PortalScaleTimer.IsTimeNotElapsed_FixedUpdateThread())
                {
                    float scalePercentage = PortalScaleTimer.RemainingDurationPercentage();
                    PortalOrb.transform.localScale = new Vector3(scalePercentage, scalePercentage, scalePercentage);
                }
                else
                {
                    PortalScaleCompleted = true;
                }
            }
            else if (!PlayerCreated)
            {
                PlayerClientData.PlayerComponent.gameObject.SetActive(true);
                PlayerOpacityTimer = new TimerStructDco_Observer(ObserverUpdateCache, ObserverUpdateCache.UpdateTickTimeFixedUpdate, (long)(PlayerOpacityDuration * 1000f));
                PlayerCreated = true;
            }
            else if (!PlayerOpaque)
            {
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
                    PlayerOpaque = true;
                }
            }
            else if (SetPlayerInactive && !PlayerDespawned)
            {
                if (PlayerOpaqueTimer.IsTimeElapsed_FixedUpdateThread())
                {
                    PlayerClientData.PlayerComponent.gameObject.SetActive(false);
                    PlayerDespawned = true;
                    Completed = true;
                }
            }
            else
            {
                PlayerClientData.PlayerComponent.gameObject.SetActive(false);
                PlayerDespawned = true;
                Completed = true;
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

    [CustomEditor(typeof(PortalBuilder))]
    public class PortalBuilderEditor : AbstractEditor
    {
        [HideInInspector]
        private PortalBuilder Instance;

        private bool VariablesSet = false;
        private bool VariablesAdded = false;

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

        public void OnDestroy()
        {
            if (VariablesAdded)
            {
                Instance.EditorDestroy();
                VariablesSet = false;
                VariablesAdded = false;
            }
        }
        public void OnDisable()
        {
            if (VariablesAdded)
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
                        PlayerComponent playerComponent = GameObject.Instantiate(playerComponentPrefab, Instance.transform);
                        playerComponent.transform.localPosition = Vector3.zero;
                        playerComponent.gameObject.SetActive(false);
                        PlayerClientData playerClientData = new PlayerClientData(playerComponent);
                        PortalOrbPurple portalOrb = GameObject.Instantiate(portalOrbPrefab, Instance.transform);
                        portalOrb.transform.localPosition = Vector3.zero;
                        portalOrb.gameObject.SetActive(false);

                        CrimsonAuraBlack crimsonAura = GameObject.Instantiate(crimsonAuraPrefab, Instance.transform);
                        crimsonAura.transform.localPosition = Vector3.zero;
                        crimsonAura.gameObject.SetActive(false);
                        SetObserverUpdateCache();
                        Instance.Initialize(ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, Instance.SetPlayerInactive);
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
            EditorGUI.EndChangeCheck();
        }
    }
}
