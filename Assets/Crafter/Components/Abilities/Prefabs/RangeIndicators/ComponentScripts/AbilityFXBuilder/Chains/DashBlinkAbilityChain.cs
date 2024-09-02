using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
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
        public TrailMoverBuilder_TargetPos BlinkTrailStartBuilder;
        [NonSerialized]
        public TrailMoverBuilder_TargetPos BlinkTrailEndBuilder;
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
            TrailMoverBuilder_TargetPos blinkTrailStartBuilder,
            TrailMoverBuilder_TargetPos blinkTrailEndBuilder,
            PlayerBlinkBuilder playerBlinkBuilderDest,
            long startTime,
            long endTime)
        {
            base.Initialize(observerUpdateCache);
            BlinkTrailStartBuilder = blinkTrailStartBuilder;
            BlinkTrailEndBuilder = blinkTrailEndBuilder;
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
                    if (!(PlayerBlinkBuilderSource.Completed && BlinkTrailStartBuilder.Completed))
                    {
                        if ((int)PlayerBlinkBuilderSource.PlayerBlinkState >= (int) PlayerBlinkState.PlayerOpacity)
                        {
                            BlinkTrailStartBuilder.ManualUpdate();
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
                if (!BlinkTrailStartBuilder.Completed)
                {
                    BlinkTrailStartBuilder.Complete();
                }
                if (!PlayerBlinkBuilderDest.Completed)
                {
                    PlayerBlinkBuilderDest.Complete();
                }
                if (!BlinkTrailEndBuilder.Completed)
                {
                    BlinkTrailEndBuilder.Complete();
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
            BlinkTrailStartBuilder = null;
            PlayerBlinkBuilderDest = null;
            BlinkTrailEndBuilder = null;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DashBlinkAbilityChain))]
    public class DashBlinkAbilityChainEditor : AbstractEditor<DashBlinkAbilityChain>
    {
        private long StartTime;
        protected override void EditorDestroy()
        {
            GameObject.DestroyImmediate(Instance.PlayerBlinkBuilderSource.gameObject);
            GameObject.DestroyImmediate(Instance.BlinkTrailStartBuilder.gameObject);
            GameObject.DestroyImmediate(Instance.BlinkTrailEndBuilder.gameObject);
            GameObject.DestroyImmediate(Instance.PlayerBlinkBuilderDest.gameObject);

            Instance.CleanUpInstance();
        }

        protected override void ManualUpdate()
        {
            long elapsedTime = ObserverUpdateCache.UpdateTickTimeRenderThread - StartTime;
            Instance.ManualUpdate(elapsedTime);
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

                    Vector3 startPosition = instance.transform.position;
                    Vector3 targetPosition = instance.transform.position + RotateY_Forward_Editor(20f, startRotationYCosYAngle, startRotationYSinYAngle);
                    playerBlinkBuilderSourceInstance.transform.position = startPosition;

                    PlayerBlinkBuilder playerBlinkBuilderDestInstance = GameObject.Instantiate(playerBlinkBuilderDestPrefab, instance.transform);
                    PlayerBlinkBuilderEditor playerBlinkBuilderDestEditor = (PlayerBlinkBuilderEditor)Editor.CreateEditor(playerBlinkBuilderDestInstance, typeof(PlayerBlinkBuilderEditor));

                    playerBlinkBuilderDestInstance.transform.position = targetPosition;

                    TrailMoverBuilder_TargetPos trailMoverBuilderTargetPosStartInstance = GameObject.Instantiate(trailMoverBuilderTargetPosPrefab, instance.transform);
                    TrailMoverBuilder_TargetPosEditor trailMoverBuilderTargetPosStartEditor = (TrailMoverBuilder_TargetPosEditor)Editor.CreateEditor(trailMoverBuilderTargetPosStartInstance,
                        typeof(TrailMoverBuilder_TargetPosEditor));
                    trailMoverBuilderTargetPosStartInstance.transform.position = startPosition;

                    TrailMoverBuilder_TargetPos trailMoverBuilderTargetPosEndInstance = GameObject.Instantiate(trailMoverBuilderTargetPosPrefab, instance.transform);
                    TrailMoverBuilder_TargetPosEditor trailMoverBuilderTargetPosEndEditor = (TrailMoverBuilder_TargetPosEditor)Editor.CreateEditor(trailMoverBuilderTargetPosEndInstance,
                        typeof(TrailMoverBuilder_TargetPosEditor));
                    trailMoverBuilderTargetPosEndInstance.transform.position = targetPosition;

                    if (observerUpdateCache == null)
                    {
                        SetObserverUpdateCache();
                        observerUpdateCache = ObserverUpdateCache;
                    }

                    playerBlinkBuilderSourceEditor.RequiredDuration = 400L;
                    playerBlinkBuilderSourceEditor.OnInspectorGUI();
                    playerBlinkBuilderSourceEditor.ForceInitialize(observerUpdateCache);

                    playerBlinkBuilderDestEditor.RequiredDuration = 400L;
                    playerBlinkBuilderDestEditor.OnInspectorGUI();
                    playerBlinkBuilderDestEditor.ForceInitialize(observerUpdateCache);

                    trailMoverBuilderTargetPosStartEditor.SetOverrides(
                        playerStartPositionOffsetOverride: Vector3.zero,
                        fullEndPositionOverride: null,
                        propsEndPositionOffsetOverride: Vector3.zero,
                        propsIndex: 1);
                    trailMoverBuilderTargetPosStartEditor.OnInspectorGUI();
                    trailMoverBuilderTargetPosStartEditor.ForceInitialize(observerUpdateCache);

                    trailMoverBuilderTargetPosEndEditor.SetOverrides(
                        playerStartPositionOffsetOverride: Vector3.zero,
                        fullEndPositionOverride: null,
                        propsEndPositionOffsetOverride: Vector3.zero,
                        propsIndex: 2);
                    trailMoverBuilderTargetPosEndEditor.OnInspectorGUI();
                    trailMoverBuilderTargetPosEndEditor.ForceInitialize(observerUpdateCache);

                    instance.Initialize(observerUpdateCache, playerBlinkBuilderSourceInstance, trailMoverBuilderTargetPosStartInstance,
                        trailMoverBuilderTargetPosEndInstance,
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
