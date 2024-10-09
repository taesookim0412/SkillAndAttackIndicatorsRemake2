using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFX;
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
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains
{
    public class PortalBuilderChain : AbstractAbilityFXBuilder
    {
        [NonSerialized]
        public PortalBuilder PortalSource;
        [NonSerialized]
        public PortalBuilder PortalDest;
        [NonSerialized]
        public TrailMoverBuilder_TargetPos TrailForPortals;

        [NonSerialized, HideInInspector]
        private long StartTime;
        [NonSerialized, HideInInspector]
        private long EndTime;

        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache,
            PortalBuilder portalSource, 
            PortalBuilder portalDest,
            TrailMoverBuilder_TargetPos trailForPortals,
            long startTime, 
            long endTime)
        {
            base.Initialize(observerUpdateCache);
            PortalSource = portalSource;
            PortalDest = portalDest;
            TrailForPortals = trailForPortals;
            StartTime = startTime;
            EndTime = endTime;
        }


        public bool ManualUpdate(long elapsedTime)
        {
            if (Completed)
            {
                return false;
            }

            if (elapsedTime >= StartTime)
            {
                if (elapsedTime <= EndTime)
                {
                    if (!TrailForPortals.Completed)
                    {
                        TrailForPortals.ManualUpdate();
                    }
                    else if (!PortalSource.Completed)
                    {
                        PortalSource.ManualUpdate();
                    }
                    else if (!PortalDest.Completed)
                    {
                        PortalDest.ManualUpdate();
                    }
                }
                else
                {
                    Complete();
                }
                return true;
            }
            else
            {
                return true;
            }
        }
        public override void Complete()
        {
            if (!Completed)
            {
                if (!TrailForPortals.Completed)
                {
                    TrailForPortals.Complete();
                }
                if (!PortalSource.Completed)
                {
                    PortalSource.Complete();
                }
                if (!PortalDest.Completed)
                {
                    PortalDest.Complete();
                }
                base.Complete();
            }
        }
        public override void CompleteStatefulFX()
        {
            if (!CompletedStateful)
            {
                PlayerComponent playerComponent = PortalDest.PlayerClientData.PlayerComponent;
                if (!playerComponent.gameObject.activeSelf)
                {
                    playerComponent.gameObject.SetActive(true);
                }
                playerComponent.transform.position = PortalDest.transform.position;

                base.CompleteStatefulFX();
            }
        }
        public override void CleanUpInstance()
        {
            PortalSource = null;
            PortalDest = null;
            TrailForPortals = null;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(PortalBuilderChain))]
    public class PortalBuilderChainEditor : AbstractEditor<PortalBuilderChain>
    {
        private long StartTime;
        protected override void EditorDestroy()
        {
            GameObject.DestroyImmediate(Instance.PortalSource.gameObject);
            GameObject.DestroyImmediate(Instance.PortalDest.gameObject);
            GameObject.DestroyImmediate(Instance.TrailForPortals.gameObject);

            Instance.CleanUpInstance();
        }

        public override void ManualUpdate()
        {
            long elapsedTime = ObserverUpdateCache.UpdateTickTimeRenderThread - StartTime;
            Instance.ManualUpdate(elapsedTime);
        }

        protected override bool OnInitialize(PortalBuilderChain instance, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {

                string portalBuilderSourceType = AbilityFXComponentType.PortalBuilder_Source.ToString();
                PortalBuilder portalBuilderSourcePrefab = (PortalBuilder)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == portalBuilderSourceType);

                string portalBuilderDestType = AbilityFXComponentType.PortalBuilder_Dest.ToString();
                PortalBuilder portalBuilderDestPrefab = (PortalBuilder)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == portalBuilderDestType);

                string trailMoverBuilderTargetPosType = AbilityFXComponentType.TrailMoverBuilder_TargetPos.ToString();
                TrailMoverBuilder_TargetPos trailMoverBuilderTargetPosPrefab = (TrailMoverBuilder_TargetPos) system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == trailMoverBuilderTargetPosType);

                if (!(portalBuilderSourcePrefab == null || portalBuilderDestPrefab == null || trailMoverBuilderTargetPosPrefab == null))
                {
                    PortalBuilder portalBuilderSourceInstance = GameObject.Instantiate(portalBuilderSourcePrefab, instance.transform);
                    PortalBuilderEditor portalBuilderSourceEditor = (PortalBuilderEditor) Editor.CreateEditor(portalBuilderSourceInstance, typeof(PortalBuilderEditor));

                    float startRotationY = instance.transform.localEulerAngles.y;
                    float startRotationYCosYAngle = (float)Math.Cos(startRotationY * Mathf.Deg2Rad);
                    float startRotationYSinYAngle = (float)Math.Sin(startRotationY * Mathf.Deg2Rad);

                    Vector3 startPosition = instance.transform.position;
                    Vector3 targetPosition = instance.transform.position + RotateY_Forward_Editor(20f, startRotationYCosYAngle, startRotationYSinYAngle);
                    portalBuilderSourceInstance.transform.position = startPosition;

                    PortalBuilder portalBuilderDestInstance = GameObject.Instantiate(portalBuilderDestPrefab, instance.transform);
                    PortalBuilderEditor portalBuilderDestEditor = (PortalBuilderEditor)Editor.CreateEditor(portalBuilderDestInstance, typeof(PortalBuilderEditor));
                    portalBuilderDestInstance.transform.position = targetPosition;

                    TrailMoverBuilder_TargetPos trailMoverBuilderTargetPosInstance = GameObject.Instantiate(trailMoverBuilderTargetPosPrefab, instance.transform);
                    TrailMoverBuilder_TargetPosEditor trailMoverBuilderTargetPosEditor = (TrailMoverBuilder_TargetPosEditor) Editor.CreateEditor(trailMoverBuilderTargetPosInstance,
                        typeof(TrailMoverBuilder_TargetPosEditor));
                    trailMoverBuilderTargetPosInstance.transform.position = startPosition + portalBuilderSourceInstance.PortalOrbOffsetPosition;

                    if (observerUpdateCache == null)
                    {
                        SetObserverUpdateCache();
                        observerUpdateCache = ObserverUpdateCache;
                    }

                    portalBuilderSourceEditor.RequiredDuration = 400L;
                    portalBuilderSourceEditor.OnInspectorGUI();
                    portalBuilderSourceEditor.ForceInitialize(observerUpdateCache);

                    portalBuilderDestEditor.RequiredDuration = 400L;
                    portalBuilderDestEditor.OnInspectorGUI();
                    portalBuilderDestEditor.ForceInitialize(observerUpdateCache);

                    trailMoverBuilderTargetPosEditor.SetOverrides( 
                        playerStartPositionOffsetOverride: portalBuilderSourceInstance.PortalOrbOffsetPosition,
                        fullEndPositionOverride: targetPosition + portalBuilderDestInstance.PortalOrbOffsetPosition,
                        propsEndPositionOffsetOverride: Vector3.zero,
                        propsIndex: 0,
                        null, 0);
                    trailMoverBuilderTargetPosEditor.OnInspectorGUI();
                    trailMoverBuilderTargetPosEditor.ForceInitialize(observerUpdateCache);

                    instance.Initialize(observerUpdateCache, portalBuilderSourceInstance, portalBuilderDestInstance, trailMoverBuilderTargetPosInstance,
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
