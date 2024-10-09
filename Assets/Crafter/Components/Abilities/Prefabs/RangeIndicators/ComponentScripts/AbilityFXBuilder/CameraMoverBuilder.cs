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
        public Transform FollowTransform;
        [NonSerialized]
        public Transform LookAtTransform;
        [SerializeField]
        public Transform CameraMoverTransform;
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
                    follow: CameraMoverTransform,
                    lookAt: lookAtTransform);
            }
        }
        public void Initialize(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            AbstractAbilityFXBuilder_Followable followableBuilder)
        {
            Transform followTransform = followableBuilder.GetFollowTransform();
            Initialize(skillAndAttackIndicatorSystem, followTransform, followTransform);
        }
        public void ManualUpdate()
        {
            CameraMoverTransform.position = FollowTransform.position + CameraOffset;
        }
        public override void Complete()
        {
            SkillAndAttackIndicatorSystem.ResetVirtualCamera();
            CameraMoverTransform.localPosition = Vector3.zero;
            CameraMoverTransform.localEulerAngles = Vector3.zero;
            base.Complete();
        }

        public override void CleanUpInstance()
        {
            SkillAndAttackIndicatorSystem = null;
            FollowTransform = null;
            LookAtTransform = null;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CameraMoverBuilder))]
    public class CameraMoverBuilderEditor : AbstractEditor<CameraMoverBuilder>
    {
        private CameraMoverBuilderProps Props;
        public AbstractAbilityFXBuilder_Followable FollowableBuilder = null;
        private bool FollowableBuilderOverride = false;
        public void SetOverrides(AbstractAbilityFXBuilder_Followable followableBuilder)
        {
            FollowableBuilder = followableBuilder;
            FollowableBuilderOverride = true;
        }
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
            }
        }

        protected override void EditorDestroy()
        {
            Instance.CameraMoverTransform.localPosition = Vector3.zero;
            Instance.CameraMoverTransform.localEulerAngles = Vector3.zero;
            Instance.CleanUpInstance();
        }

        public override void ManualUpdate()
        {
            if (!Instance.Completed)
            {
                // Try to reset the virtual camera.
                Instance.Complete();
            }

            SceneView sceneView = SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.lastActiveSceneView;
            }

            Transform cameraMoverTransform = Instance.CameraMoverTransform;
            cameraMoverTransform.position = Instance.FollowTransform.position + Instance.CameraOffset;
            cameraMoverTransform.LookAt(Instance.LookAtTransform);
            sceneView.AlignViewToObject(cameraMoverTransform);
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
                if (FollowableBuilderOverride)
                {
                    instance.Initialize(system, FollowableBuilder);
                }
                else
                {
                    instance.Initialize(system, Props.FollowTransform, Props.LookAtTransform);
                }

                TryAddParticleSystem(instance.gameObject);

                return true;
            }
            return false;
        }
    }
#endif
}

