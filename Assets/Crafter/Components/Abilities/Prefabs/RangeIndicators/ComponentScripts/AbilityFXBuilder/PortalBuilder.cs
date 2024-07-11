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
        public PlayerClientData PlayerClientData;
        [NonSerialized]
        public PortalOrbPurple PortalOrb;
        [NonSerialized]
        public CrimsonAuraBlack CrimsonAura;
    }

    [CustomEditor(typeof(PortalBuilder))]
    public class PortalBuilderEditor : Editor
    {
        [HideInInspector]
        private PortalBuilder Editor;

        private bool VariablesSet = false;
        public void OnSceneGUI()
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
                    Editor.PlayerClientData = new PlayerClientData(instance.PlayerComponent);
                    string portalOrbPurpleType = AbilityFXComponentType.PortalOrbPurple.ToString();
                    Editor.PortalOrb = (PortalOrbPurple)instance.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == portalOrbPurpleType);
                    string crimsonAuraBlackType = AbilityFXComponentType.CrimsonAuraBlack.ToString();
                    Editor.CrimsonAura = (CrimsonAuraBlack)instance.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == crimsonAuraBlackType);

                    if (Editor.PortalOrb != null && Editor.CrimsonAura != null)
                    {
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

            if (VariablesSet)
            {

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
