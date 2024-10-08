using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class CameraMoverBuilder : AbstractAbilityFXBuilder
    {
        [NonSerialized]
        private SkillAndAttackIndicatorSystem SkillAndAttackIndicatorSystem;
        [NonSerialized]
        private Transform FollowTransform;
        [NonSerialized]
        private Transform LookAtTransform;
        [SerializeField]
        public Vector3 CameraOffset = new Vector3(1f, 3f, -2.5f);
        public override void ManualAwake()
        {

        }
        public void Initialize(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, 
            Transform followTransform,
            Transform lookAtTransform)
        {
            base.Initialize();
            SkillAndAttackIndicatorSystem = skillAndAttackIndicatorSystem;
            FollowTransform = followTransform;
            LookAtTransform = lookAtTransform;
            if (!(followTransform == null || lookAtTransform == null))
            {
                skillAndAttackIndicatorSystem.SetPlayerMoverCamera(
                    follow: followTransform,
                    lookAt: lookAtTransform);
            }

        }
        public override void Complete()
        {
            SkillAndAttackIndicatorSystem.ResetVirtualCamera();
            base.Complete();
        }

        public override void CleanUpInstance()
        {
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CameraMoverBuilder))]
    public class CameraMoverBuilderEditor : AbstractEditor<CameraMoverBuilder>
    {
        private CameraMoverBuilderProps Props;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (Props == null)
            {
                Props = FindProps<CameraMoverBuilderProps>();
            }

            if (Props != null)
            {
                Props.FollowTransform = (Transform)EditorGUILayout.ObjectField("Follow Transform", Props.FollowTransform, typeof(Transform), true);
                Props.LookAtTransform = (Transform)EditorGUILayout.ObjectField("LookAt Transform", Props.LookAtTransform, typeof(Transform), true);
                Props.SceneViewObject = (Transform)EditorGUILayout.ObjectField("SceneViewObject", Props.SceneViewObject, typeof(Transform), true);
            }
        }

        protected override void EditorDestroy()
        {
            Instance.CleanUpInstance();
        }

        protected override void ManualUpdate()
        {
            if (!Instance.Completed)
            {
                // Try to reset the virtual camera's transform refs.
                Instance.Complete();
            }

            SceneView sceneView = SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.lastActiveSceneView;
            }

            Props.FollowTransform.position = Props.LookAtTransform.position + Instance.CameraOffset;
            Props.SceneViewObject.position = Props.FollowTransform.position;
            Props.SceneViewObject.LookAt(Props.LookAtTransform);
            sceneView.AlignViewToObject(Props.SceneViewObject);
        }

        protected override bool OnInitialize(CameraMoverBuilder instance, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();

            if (system != null && Props != null)
            {
                if (observerUpdateCache == null)
                {
                    SetObserverUpdateCache();
                    observerUpdateCache = ObserverUpdateCache;
                }

                instance.Initialize(system, Props.FollowTransform, Props.LookAtTransform);
                TryAddParticleSystem(instance.gameObject);

                return true;
            }
            return false;
        }
    }
#endif
}

