using Assets.Crafter.Components.Constants;
using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Editors.Helpers;
using Assets.Crafter.Components.Models.dpo.TrailEffectsDpo;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains
{
    public class DashBlinkAbilityChain: AbstractAbilityFXBuilder
    {
        [NonSerialized]
        public PlayerClientData PlayerClientData;
        [NonSerialized]
        public PlayerComponent PlayerTransparentClone;
        [NonSerialized]
        public TrailMoverBuilder_TargetPos BlinkTrailBuilder;
        [NonSerialized]
        public PlayerBlinkBuilder PlayerBlinkBuilderSource;
        [NonSerialized]
        public PlayerBlinkBuilder PlayerBlinkBuilderDest;

        [NonSerialized, HideInInspector]
        private long StartTime;
        [NonSerialized, HideInInspector]
        private long EndTime;
        public override void ManualAwake()
        {
        }
        public void Initialize(ObserverUpdateCache observerUpdateCache,
            PlayerBlinkBuilder playerBlinkBuilderSource,
            TrailMoverBuilder_TargetPos blinkTrailBuilder,
            PlayerBlinkBuilder playerBlinkBuilderDest,
            long startTime,
            long endTime)
        {
            base.Initialize(observerUpdateCache);
            BlinkTrailBuilder = blinkTrailBuilder;
            PlayerBlinkBuilderSource = playerBlinkBuilderSource;
            PlayerBlinkBuilderDest = playerBlinkBuilderDest;
            StartTime = startTime;
            EndTime = endTime;
        }

        public void ManualUpdate(long elapsedTime)
        {
            if (Completed)
            {
                return;
            }

            if (elapsedTime >= StartTime)
            {
                if (elapsedTime <= EndTime)
                {
                    if (!(PlayerBlinkBuilderSource.Completed && BlinkTrailBuilder.Completed))
                    {
                        if ((int)PlayerBlinkBuilderSource.PlayerBlinkState >= (int)PlayerBlinkState.PlayerOpacity)
                        {
                            BlinkTrailBuilder.ManualUpdate();
                        }

                        PlayerBlinkBuilderSource.ManualUpdate();
                    }
                    else if (!PlayerBlinkBuilderDest.Completed)
                    {
                        PlayerBlinkBuilderDest.ManualUpdate();
                    }
                }
                else
                {
                    Complete();
                }
            }
        }
        public override void Complete()
        {
            if (!Completed)
            {
                if (!PlayerBlinkBuilderSource.Completed)
                {
                    PlayerBlinkBuilderSource.Complete();
                }
                if (!BlinkTrailBuilder.Completed)
                {
                    BlinkTrailBuilder.Complete();
                }
                if (!PlayerBlinkBuilderDest.Completed)
                {
                    PlayerBlinkBuilderDest.Complete();
                }
                base.Complete();
            }
        }
        public override void CompleteStatefulFX()
        {
            if (!CompletedStateful)
            {
                PlayerComponent playerComponent = PlayerBlinkBuilderDest.PlayerClientData.PlayerComponent;
                if (!playerComponent.gameObject.activeSelf)
                {
                    playerComponent.gameObject.SetActive(true);
                }
                playerComponent.transform.position = PlayerBlinkBuilderDest.transform.position;

                base.CompleteStatefulFX();
            }
        }
        public override void CleanUpInstance()
        {
            PlayerBlinkBuilderSource = null;
            BlinkTrailBuilder = null;
            PlayerBlinkBuilderDest = null;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DashBlinkAbilityChain))]
    public class DashBlinkAbilityChainEditor : AbstractEditor<DashBlinkAbilityChain>
    {
        private DashBlinkAbilityChainEditorProps Props;

        private long StartTime;
        protected override void EditorDestroy()
        {
            GameObject.DestroyImmediate(Instance.PlayerBlinkBuilderSource.gameObject);
            GameObject.DestroyImmediate(Instance.BlinkTrailBuilder.gameObject);
            GameObject.DestroyImmediate(Instance.PlayerBlinkBuilderDest.gameObject);

            Instance.CleanUpInstance();
        }

        protected override void ManualUpdate()
        {
            long elapsedTime = ObserverUpdateCache.UpdateTickTimeRenderThread - StartTime;
            Instance.ManualUpdate(elapsedTime);
        }
        //public void OnSceneGUI()
        //{
        //    if (Instance.Animator != null)
        //    {
        //        Instance.Animator.Play(Instance.AnimStateName, Instance.AnimLayerIndex, Instance.AnimClipFrame);
        //        Instance.Animator.Update(0.166f);
        //    }
        //}

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (Props == null)
            {
                Props = GameObject.Find($"{Instance.name}Props").GetComponent<DashBlinkAbilityChainEditorProps>();
            }

            if (Props != null) 
            {
                Props.PlayerBlinkSourceTargetPos = EditorGUILayout.Vector3Field("PlayerBlinkSourceTargetPos", Props.PlayerBlinkSourceTargetPos);
                Props.PlayerBlinkDestTargetPos = EditorGUILayout.Vector3Field("PlayerBlinkDestTargetPos", Props.PlayerBlinkDestTargetPos);
                EditorGUI.BeginChangeCheck();
                Props.PlayerComponent = (PlayerComponent)EditorGUILayout.ObjectField("PlayerComponent", Props.PlayerComponent, typeof(PlayerComponent), true);
                Props.Animator = (Animator)EditorGUILayout.ObjectField("Animator", Props.Animator, typeof(Animator), true);
                bool changeAnimator = EditorGUI.EndChangeCheck();

                if (Props.Animator != null)
                {
                    DrawAnimTab(changeAnimator);
                }
            }
            
        }
        private void DrawAnimTab(bool changeAnimator)
        {
            EditorGUI.BeginChangeCheck();
            Props.AnimLayerIndex = EditorGUILayout.IntField("AnimLayerIndex", Props.AnimLayerIndex);
            Props.AnimStateName = EditorGUILayout.TextField("State name", Props.AnimStateName);
            if (EditorGUI.EndChangeCheck() || changeAnimator)
            {
                Props.AnimClip = PartialEditorHelpers.GetAnimStateClip((AnimatorController)Props.Animator.runtimeAnimatorController, Props.AnimLayerIndex, Props.AnimStateName);
            }

            float selectedClipLength;
            if (Props.AnimClip != null)
            {
                selectedClipLength = Props.AnimClip.length;
            }
            else
            {
                return;
            }

            Props.AnimClipFrame = EditorGUILayout.Slider("AnimClipFrame", Props.AnimClipFrame, 0f, selectedClipLength);


        }

        protected override bool OnInitialize(DashBlinkAbilityChain instance, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {

                string playerBlinkBuilderSourceType = AbilityFXComponentType.PlayerBlinkBuilder_Source.ToString();
                PlayerBlinkBuilder playerBlinkBuilderSourcePrefab = (PlayerBlinkBuilder)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == playerBlinkBuilderSourceType);

                string playerBlinkBuilderDestType = AbilityFXComponentType.PlayerBlinkBuilder_Dest.ToString();
                PlayerBlinkBuilder playerBlinkBuilderDestPrefab = (PlayerBlinkBuilder)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == playerBlinkBuilderDestType);

                string trailMoverBuilderTargetPosType = AbilityFXComponentType.TrailMoverBuilder_TargetPos.ToString();
                TrailMoverBuilder_TargetPos trailMoverBuilderTargetPosPrefab = (TrailMoverBuilder_TargetPos)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == trailMoverBuilderTargetPosType);

                if (!(playerBlinkBuilderSourceType == null || trailMoverBuilderTargetPosPrefab == null || trailMoverBuilderTargetPosPrefab == null))
                {
                    PlayerBlinkBuilder playerBlinkBuilderSourceInstance = GameObject.Instantiate(playerBlinkBuilderSourcePrefab, instance.transform);
                    PlayerBlinkBuilderEditor playerBlinkBuilderSourceEditor = (PlayerBlinkBuilderEditor)Editor.CreateEditor(playerBlinkBuilderSourceInstance, typeof(PlayerBlinkBuilderEditor));

                    float startRotationY = instance.transform.localEulerAngles.y;
                    float startRotationYCosYAngle = (float)Math.Cos(startRotationY * Mathf.Deg2Rad);
                    float startRotationYSinYAngle = (float)Math.Sin(startRotationY * Mathf.Deg2Rad);

                    Vector3 endPositionOffset = RotateY_Forward_Editor(20f, startRotationYCosYAngle, startRotationYSinYAngle);
                    Vector3 startPosition = instance.transform.position;
                    Vector3 targetPosition = instance.transform.position + endPositionOffset;
                    playerBlinkBuilderSourceInstance.transform.position = startPosition;

                    PlayerBlinkBuilder playerBlinkBuilderDestInstance = GameObject.Instantiate(playerBlinkBuilderDestPrefab, instance.transform);
                    PlayerBlinkBuilderEditor playerBlinkBuilderDestEditor = (PlayerBlinkBuilderEditor)Editor.CreateEditor(playerBlinkBuilderDestInstance, typeof(PlayerBlinkBuilderEditor));

                    playerBlinkBuilderDestInstance.transform.position = targetPosition;

                    TrailMoverBuilder_TargetPos trailMoverBuilderTargetPosInstance = GameObject.Instantiate(trailMoverBuilderTargetPosPrefab, instance.transform);
                    TrailMoverBuilder_TargetPosEditor trailMoverBuilderTargetPosEditor = (TrailMoverBuilder_TargetPosEditor)Editor.CreateEditor(trailMoverBuilderTargetPosInstance,
                        typeof(TrailMoverBuilder_TargetPosEditor));
                    trailMoverBuilderTargetPosInstance.transform.position = startPosition;

                    if (observerUpdateCache == null)
                    {
                        SetObserverUpdateCache();
                        observerUpdateCache = ObserverUpdateCache;
                    }

                    (Vector3 localPos, bool useEndPosition)[] trailVertexPositionsLocal = new (Vector3 localPos, bool useEndPosition)[2] { 
                        (Props.PlayerBlinkSourceTargetPos, false), 
                        (Props.PlayerBlinkDestTargetPos, true) };

                    trailMoverBuilderTargetPosEditor.SetOverrides(
                        playerStartPositionOffsetOverride: Vector3.zero,
                        fullEndPositionOverride: null,
                        propsEndPositionOffsetOverride: endPositionOffset,
                        propsIndex: 1,
                        trailVertexPositionsLocal: trailVertexPositionsLocal,
                        trailVertexTrailIndex: 0);
                    trailMoverBuilderTargetPosEditor.OnInspectorGUI();
                    trailMoverBuilderTargetPosEditor.ForceInitialize(observerUpdateCache);

                    playerBlinkBuilderSourceEditor.SetOverrides(Props.PlayerComponent, trailMoverBuilderTargetPosInstance.ClosestTrailPositionsLocal[0]);
                    playerBlinkBuilderSourceEditor.RequiredDuration = 400L;
                    playerBlinkBuilderSourceEditor.OnInspectorGUI();
                    playerBlinkBuilderSourceEditor.ForceInitialize(observerUpdateCache);

                    playerBlinkBuilderDestEditor.SetOverrides(Props.PlayerComponent, trailMoverBuilderTargetPosInstance.ClosestTrailPositionsLocal[1]);
                    playerBlinkBuilderDestEditor.RequiredDuration = 400L;
                    playerBlinkBuilderDestEditor.OnInspectorGUI();
                    playerBlinkBuilderDestEditor.ForceInitialize(observerUpdateCache);

                    instance.Initialize(observerUpdateCache, playerBlinkBuilderSourceInstance, trailMoverBuilderTargetPosInstance,
                        playerBlinkBuilderDestInstance,
                        startTime: 0L,
                        endTime: 1800L);

                    TryAddParticleSystem(instance.gameObject);

                    StartTime = observerUpdateCache.UpdateTickTimeRenderThread;
                    return true;
                }
            }
            return false;
        }
    }

#endif
}
