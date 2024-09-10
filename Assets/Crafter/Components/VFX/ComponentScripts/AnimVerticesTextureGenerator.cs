using Assets.Crafter.Components.Editors.Helpers;
using Assets.Crafter.Components.Player.ComponentScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Crafter.Components.VFX.ComponentScripts
{
    public class AnimVerticesTextureGenerator : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public PlayerComponent PlayerComponent;
        [SerializeField, HideInInspector]
        public Animator Animator;
        [SerializeField, HideInInspector]
        public string AnimStateName;
        [SerializeField, HideInInspector]
        public int AnimLayerIndex;
        [SerializeField, HideInInspector]
        public AnimationClip AnimClip;
        [SerializeField, HideInInspector]
        public float AnimClipFrame;
    }

    [CustomEditor(typeof(AnimVerticesTextureGenerator))]
    public class AnimVerticesTextureGeneratorEditor : Editor
    {
        public AnimVerticesTextureGenerator Instance;
        public void OnSceneGUI()
        {
            if (Instance.Animator != null)
            {
                Instance.Animator.Play(Instance.AnimStateName, Instance.AnimLayerIndex, Instance.AnimClipFrame);
                Instance.Animator.Update(0.166f);
            }
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Instance = (AnimVerticesTextureGenerator)target;

            Undo.RecordObject(Instance, "All State");

            EditorGUI.BeginChangeCheck();
            Instance.PlayerComponent = (PlayerComponent)EditorGUILayout.ObjectField("PlayerComponent", Instance.PlayerComponent, typeof(PlayerComponent), true);
            Instance.Animator = (Animator)EditorGUILayout.ObjectField("Animator", Instance.Animator, typeof(Animator), true);
            bool changeAnimator = EditorGUI.EndChangeCheck();

            if (Instance.Animator != null)
            {
                DrawAnimTab(changeAnimator);
            }
        }
        private void DrawAnimTab(bool changeAnimator)
        {
            EditorGUI.BeginChangeCheck();
            Instance.AnimLayerIndex = EditorGUILayout.IntField("AnimLayerIndex", Instance.AnimLayerIndex);
            Instance.AnimStateName = EditorGUILayout.TextField("State name", Instance.AnimStateName);
            if (EditorGUI.EndChangeCheck() || changeAnimator)
            {
                Instance.AnimClip = PartialEditorHelpers.GetAnimStateClip((AnimatorController)Instance.Animator.runtimeAnimatorController, Instance.AnimLayerIndex, Instance.AnimStateName);
            }

            float selectedClipLength;
            if (Instance.AnimClip != null) 
            {
                selectedClipLength = Instance.AnimClip.length;
            }
            else
            {
                return;
            }

            Instance.AnimClipFrame = EditorGUILayout.Slider("AnimClipFrame", Instance.AnimClipFrame, 0f, selectedClipLength);


        }
    }
}
