using Assets.Crafter.Components.Editors.ComponentScripts;
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

        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PortalOrbPurple portalOrb, CrimsonAuraBlack crimsonAura)
        {
            ObserverUpdateCache = observerUpdateCache;
            PlayerClientData = playerClientData;
            PortalOrb = portalOrb;
            CrimsonAura = crimsonAura;
        }
        public void ManualUpdate()
        {

        }

        public void EditorDestroy()
        {
            ObserverUpdateCache = null;
            PlayerClientData = null;
            PortalOrb = null;
            CrimsonAura = null;
        }
    }

    [CustomEditor(typeof(PortalBuilder))]
    public class PortalBuilderEditor : AbstractEditor
    {
        [HideInInspector]
        private PortalBuilder Editor;

        private bool VariablesSet = false;
        private bool VariablesAdded = false;
        public void OnSceneGUI()
        {
            Initialize();

            if (VariablesSet)
            {

            }
        }

        public void OnDestroy()
        {
            if (VariablesAdded)
            {
                Editor.EditorDestroy();
            }
        }

        private void Initialize()
        {
            if (!VariablesSet)
            {
                VariablesSet = Editor != null && Editor.PlayerClientData != null && Editor.PortalOrb != null && Editor.CrimsonAura != null;
            }
            if (!VariablesSet)
            {
                SkillAndAttackIndicatorSystem instance = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
                if (instance != null)
                {
                    PlayerClientData playerClientData = new PlayerClientData(instance.PlayerComponent);

                    string portalOrbPurpleType = AbilityFXComponentType.PortalOrbPurple.ToString();
                    PortalOrbPurple portalOrb = (PortalOrbPurple)instance.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == portalOrbPurpleType);

                    string crimsonAuraBlackType = AbilityFXComponentType.CrimsonAuraBlack.ToString();
                    CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack)instance.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == crimsonAuraBlackType);

                    if (playerClientData != null && portalOrb != null && crimsonAura != null)
                    {
                        SetObserverUpdateCache();
                        Editor.Initialize(ObserverUpdateCache, playerClientData, portalOrb, crimsonAura);
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
            Editor = (PortalBuilder)target;

            Undo.RecordObject(Editor, "Editor State");

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            EditorGUI.BeginChangeCheck();
            PlayerComponent playerComponent = (PlayerComponent) EditorGUILayout.ObjectField("PlayerComponent", Editor.PlayerClientData != null ? Editor.PlayerClientData.PlayerComponent : null, typeof(PlayerComponent), true);
            PortalOrbPurple portalOrbPurple = (PortalOrbPurple) EditorGUILayout.ObjectField("PortalOrb", Editor.PortalOrb, typeof(PortalOrbPurple), true);
            CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack) EditorGUILayout.ObjectField("CrimsonAura", Editor.CrimsonAura, typeof(CrimsonAuraBlack), true);
            EditorGUI.EndChangeCheck();
        }
    }
}
