using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFX;
using Assets.Crafter.Components.Constants;
using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Models.dco;
using Assets.Crafter.Components.Models.dpo.TrailEffectsDpo;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class TrailMoverBuilder_TargetPos : AbstractAbilityFXBuilder
    {
        [NonSerialized]
        public BlinkRibbonTrailRenderer[] Trails;
        [NonSerialized, HideInInspector]
        public BlinkRibbonTrailProps TrailProps;
        [NonSerialized, HideInInspector]
        public Vector3[] TrailStartPositions;
        [NonSerialized, HideInInspector]
        public bool[] TrailStarted;
        
        [NonSerialized, HideInInspector]
        public Vector3[][] TrailPositions;


        [NonSerialized, HideInInspector]
        private float ElapsedTimeSec;
        [NonSerialized, HideInInspector]
        private float TimeRequiredSec;

        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache,
            SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            Vector3 playerStartOffsetPosition,
            BlinkRibbonTrailRenderer[] trails,
            BlinkRibbonTrailProps blinkRibbonTrailProps,
            float startRotationY,
            float startRotationYCosYAngle,
            float startRotationYSinYAngle,
            float timeRequiredSec,
            Vector3 endPositionWorld)
        {
            base.Initialize(observerUpdateCache);

            Vector3 startWorldPosition = transform.position;

            float[] widthMultipliers = blinkRibbonTrailProps.WidthMultipliers; 
            Vector3[] localStartPositionOffsets = blinkRibbonTrailProps.StartPositionOffsetsLocal;
            Vector3[] trailStartPositions = new Vector3[trails.Length];
            bool[] trailStarted = new bool[trails.Length];
            for (int i = 0; i < trails.Length; i++)
            {
                Vector3 startPositionOffset = localStartPositionOffsets[i] + playerStartOffsetPosition;
                float startPositionOffsetX = startPositionOffset.x;
                float startPositionOffsetZ = startPositionOffset.z;

                float rotatedLocalPositionX = startPositionOffsetZ * startRotationYSinYAngle + startPositionOffsetX * startRotationYCosYAngle;
                float rotatedLocalPositionZ = startPositionOffsetZ * startRotationYCosYAngle - startPositionOffsetX * startRotationYSinYAngle;

                BlinkRibbonTrailRenderer trail = trails[i];
                trail.SetTrailRendererWidth(widthMultipliers[i]);
                float worldPositionX = startWorldPosition.x + rotatedLocalPositionX;
                float worldPositionZ = startWorldPosition.z + rotatedLocalPositionZ;
                float worldPositionY = skillAndAttackIndicatorSystem.GetTerrainHeight(worldPositionX, worldPositionZ) + startPositionOffset.y;

                Vector3 startPosition = new Vector3(worldPositionX,
                    worldPositionY,
                    worldPositionZ);
                trailStartPositions[i] = startPosition;
                trail.transform.position = startPosition;
                trail.TrailRenderer.Clear();

                trailStarted[i] = false;
            }
            TrailStartPositions = trailStartPositions;
            TrailStarted = trailStarted;

            Vector3[] localStartRotationOffsets = blinkRibbonTrailProps.StartRotationOffsetsLocal;
            Vector3[] movementRotations = new Vector3[trails.Length];
            for (int i = 0; i < trails.Length; i++)
            {
                Vector3 startRotation = localStartRotationOffsets[i].Copy();
                startRotation.y += startRotationY;
                movementRotations[i] = startRotation;
            }

            Trails = trails;
            TrailProps = blinkRibbonTrailProps;

            Vector3[][] allTrailMarkersWorldAndEndPosition = new Vector3[trails.Length][];
            for (int i = 0; i < trails.Length; i++)
            {
                Vector3[] trailMarkersLocal = blinkRibbonTrailProps.TrailMarkersLocal[i].Items;
                Vector3[] trailMarkersWorldAndEndPosition = new Vector3[trailMarkersLocal.Length + 1];
                for (int j = 0; j < trailMarkersWorldAndEndPosition.Length - 1; j++) 
                {
                    Vector3 positionOffset = trailMarkersLocal[j];
                    float positionOffsetX = positionOffset.x;
                    float positionOffsetZ = positionOffset.z;

                    float rotatedPositionOffsetX = positionOffsetZ * startRotationYSinYAngle + positionOffsetX * startRotationYCosYAngle;
                    float rotatedPositionOffsetZ = positionOffsetZ * startRotationYCosYAngle - positionOffsetX * startRotationYSinYAngle;

                    float worldPositionX = startWorldPosition.x + rotatedPositionOffsetX;
                    float worldPositionZ = startWorldPosition.z + rotatedPositionOffsetZ;

                    float worldPositionY = skillAndAttackIndicatorSystem.GetTerrainHeight(worldPositionX, worldPositionZ) + positionOffset.y;
                    trailMarkersWorldAndEndPosition[j] = new Vector3(worldPositionX,
                        worldPositionY,
                        worldPositionZ);
                }

                // end position is not set yet.
                allTrailMarkersWorldAndEndPosition[i] = trailMarkersWorldAndEndPosition;
            }

            SetEndPositionsWorld(startRotationY, endPositionWorld, startRotationYCosYAngle, startRotationYSinYAngle, allTrailMarkersWorldAndEndPosition);

            ElapsedTimeSec = 0f;
            TimeRequiredSec = timeRequiredSec;

            TrailPositions = CreateTrailPositions(trails, allTrailMarkersWorldAndEndPosition, movementRotations, trailStartPositions, timeRequiredSec,
                velocity: blinkRibbonTrailProps.Velocity);
        }

        public void SetEndPositionsWorld(float startYRotation, Vector3 newEndPositionWorld,
            float startRotationYCosYAngle,
            float startRotationYSinYAngle,
            Vector3[][] allTrailMarkersWorldAndEndPosition)
        {
            Vector3[] localEndPositionOffsets = TrailProps.EndPositionOffsetsLocal;

            for (int i = 0; i < localEndPositionOffsets.Length; i++)
            {
                Vector3 endPositionOffset = localEndPositionOffsets[i];
                float endPositionOffsetX = endPositionOffset.x;
                float endPositionOffsetZ = endPositionOffset.z;

                float rotatedEndPositionX = endPositionOffsetZ * startRotationYSinYAngle + endPositionOffsetX * startRotationYCosYAngle;
                float rotatedEndPositionZ = endPositionOffsetZ * startRotationYCosYAngle - endPositionOffsetX * startRotationYSinYAngle;
                Vector3 endPosition = new Vector3(newEndPositionWorld.x + rotatedEndPositionX, newEndPositionWorld.y + endPositionOffset.y, newEndPositionWorld.z + rotatedEndPositionZ);
                
                Vector3[] trailMarkerWorldAndEndPosition = allTrailMarkersWorldAndEndPosition[i];
                trailMarkerWorldAndEndPosition[trailMarkerWorldAndEndPosition.Length - 1] = endPosition;
                //Debug.Log(newEndPositionWorld);
                //Debug.Log(endPositions[i]);
            }
        }

        public void ManualUpdate()
        {
            if (Completed)
            {
                return;
            }
            if (!Active)
            {
                Active = true;
            }
            float elapsedDeltaTime = ObserverUpdateCache.UpdateTickTimeRenderThreadDeltaTimeSec;
            if (ElapsedTimeSec < TimeRequiredSec)
            {
                MoveTrailToEndPositions(elapsedDeltaTime, ElapsedTimeSec, out bool _completed);
                if (_completed)
                {
                    Completed = true;
                }
            }
            else
            {
                Completed = true;
            }

            ElapsedTimeSec += elapsedDeltaTime;
        }

        public void MoveTrailToEndPositions(float elapsedDeltaTime, float previousElapsedTimeSec, out bool completed)
        {
            float destElapsedTimeSec = previousElapsedTimeSec + elapsedDeltaTime;

            int startIndex = Math.Max((int) (previousElapsedTimeSec * SkillAndAttackIndicatorSystem.FixedTrailTimestepSecReciprocal), 0);
            int endIndex = (int)(destElapsedTimeSec * SkillAndAttackIndicatorSystem.FixedTrailTimestepSecReciprocal);

            completed = true;
            // very simple with no interpolation between remainder dt.
            BlinkRibbonTrailRenderer[] trails = Trails;
            for (int i = 0; i < trails.Length; i++)
            {
                bool trailCompleted = false;
                Vector3[] trailPositions = TrailPositions[i];

                if (startIndex == 0 && !TrailStarted[i])
                {
                    BlinkRibbonTrailRenderer trail = trails[i];
                    Vector3 startPosition = TrailStartPositions[i];
                    trail.TrailRenderer.AddPosition(startPosition);
                    trail.transform.position = startPosition;

                    TrailStarted[i] = true;
                }
                if (startIndex + 1 < trailPositions.Length) 
                {
                    BlinkRibbonTrailRenderer trail = trails[i];

                    int clampedEndIndex = endIndex;
                    if (clampedEndIndex >= trailPositions.Length)
                    {
                        clampedEndIndex = trailPositions.Length - 1;
                    }
                    TrailRenderer trailRenderer = trail.TrailRenderer;
                    for (int j = startIndex + 1; j < clampedEndIndex + 1; j++)
                    {
                        trailRenderer.AddPosition(trailPositions[j]);
                        trail.transform.position = trailPositions[j];
                    }
                    if (clampedEndIndex >= trailPositions.Length - 1)
                    {
                        trailCompleted = true;
                    }
                }
                else
                {
                    trailCompleted = true;
                }

                // completed = completed && trailCompleted without the extra reassignment.
                if (completed)
                {
                    completed = trailCompleted;
                }
            }
        }

        public Vector3[][] CreateTrailPositions(BlinkRibbonTrailRenderer[] blinkTrails,
            Vector3[][] trailMarkersWorldAndEndPosition,
            Vector3[] movementRotations,
            Vector3[] trailStartPositions,
            float timeRequiredSec,
            float velocity)
        {
            float timestepDeltaTime = SkillAndAttackIndicatorSystem.FixedTrailTimestepSec;
            Vector3[][] allTrailPositions = new Vector3[blinkTrails.Length][];

            int positions = (int) (timeRequiredSec * SkillAndAttackIndicatorSystem.FixedTrailTimestepSecReciprocal);

            for (int i = 0; i < blinkTrails.Length; i++)
            {
                Vector3 rotatingAnglesForwardVector = Vector3.forward;
                Vector3 movementRotation = movementRotations[i];
                Vector3 blinkTrailPosition = trailStartPositions[i];
                Vector3[] trailMarkers = trailMarkersWorldAndEndPosition[i];
                int trailMarkerIndex = 0;
                int stopIndex = positions - 1;

                float targetDistance = timestepDeltaTime * velocity;
                float rotationSpeed = 0.8f;

                bool useLastPositionTargetDistance = false;
                float lastPositionTargetDistance = targetDistance;

                Vector3[] trailForwardVectorsNormalizedFullSize = new Vector3[positions];
                for (int j = 0; j < positions; j++)
                {
                    if (trailMarkerIndex < trailMarkers.Length)
                    {
                        for (; trailMarkerIndex < trailMarkers.Length; trailMarkerIndex++)
                        {
                            Vector3 destPosition = trailMarkers[trailMarkerIndex];
                            //Debug.Log($"{i}: {currentPosition}, {destPosition}, previousMovementRotation: {movementRotation}");

                            Vector3 directionVector = destPosition - blinkTrailPosition;
                            float destPositionDistance = directionVector.magnitude;

                            bool destPositionReached = destPositionDistance <= 0.1f;

                            if (targetDistance > destPositionDistance)
                            {
                                if (trailMarkerIndex != trailMarkers.Length - 1)
                                {
                                    continue;
                                }
                                else
                                {
                                    lastPositionTargetDistance = destPositionDistance;
                                    useLastPositionTargetDistance = true;
                                    destPositionReached = true;
                                }

                                // dp = v * dt
                                // dt = dp * 1/v
                                //float velocityReciprocal = 1 / 35f;
                                //trailElapsedDeltaTime -= destPositionDistance * velocityReciprocal;

                                //targetDistance = destPositionDistance;

                                //destPositionReached = true;
                                //Debug.Log($"{i}: {1}");
                            }
                            else
                            {
                                if (destPositionDistance > PartialMathUtil.FLOAT_TOLERANCE)
                                {
                                    float distanceScalar = targetDistance / destPositionDistance;
                                    directionVector = distanceScalar * directionVector;
                                    //Debug.Log($"{i}: {2}");
                                }
                                else
                                {
                                    continue;
                                }
                                //Debug.Log($"{i}: {3}");
                            }
                            //Debug.Log($"{i}: {directionVector}");

                            Vector3 rotation = Vector3Util.LookRotationPitchYaw(directionVector);

                            //float timeElapsedPercentage = trailElapsedTimeSec * timeRequiredSecReciprocal;
                            //float easeTimePercentage = EffectsUtil.EaseInOutQuad(timeElapsedPercentage);

                            float rotationXDifference = PartialMathUtil.DeltaAngle(movementRotation.x, rotation.x);
                            float rotationYDifference = PartialMathUtil.DeltaAngle(movementRotation.y, rotation.y);

                            movementRotation.x = PartialMathUtil.LerpDeltaAngle(movementRotation.x, rotationXDifference, rotationSpeed);
                            //float maxMovementRotationX = PartialMathUtil.LerpDeltaAngle(movementRotation.x, rotationXDifference, trailElapsedTimeSec * rotationSpeedMultiplier);
                            //movementRotation.x = PositionUtil.CalculateClosestMultipleOrClamp(movementRotation.x, maxMovementRotationX, elapsedDeltaTime);
                            //movementRotation.x = movementRotation.x + rotationXDifference * easeTimePercentage;

                            movementRotation.y = PartialMathUtil.LerpDeltaAngle(movementRotation.y, rotationYDifference, rotationSpeed);
                            //float maxMovementRotationY = PartialMathUtil.LerpDeltaAngle(movementRotation.y, rotationYDifference, trailElapsedTimeSec * rotationSpeedMultiplier);
                            //movementRotation.y = PositionUtil.CalculateClosestMultipleOrClamp(movementRotation.y, maxMovementRotationY, elapsedDeltaTime);
                            //movementRotation.y = movementRotation.y + rotationYDifference * easeTimePercentage;

                            RotatingCoordinateXYVector3Angles rotatingAngles = new RotatingCoordinateXYVector3Angles(movementRotation);
                            Vector3 rotatingAnglesForwardVectorNormalized = rotatingAngles.RotateXY_Forward();
                            //Debug.Log($"{i}: {movementRotation}, {directionVector}, {endPosition}, {currentPosition}");

                            trailForwardVectorsNormalizedFullSize[j] = rotatingAnglesForwardVectorNormalized;

                            rotatingAnglesForwardVector = rotatingAnglesForwardVectorNormalized * targetDistance;
                            // this will be short because the total distance is not curved so it needs to be faster ( mult by 5 ).
                            //float targetVelocity = totalDirectionDistance * timeRequiredSecReciprocal * 5f;
                            //float dtxTargetVelocity = trailElapsedDeltaTime * targetVelocity;



                            float nextMaxXValue = blinkTrailPosition.x + rotatingAnglesForwardVector.x;
                            //float clampedMaxXValue = PartialMathUtil.DirectionMaxValueClamped(trailMarkerIndex == 0 ? StartPositions[i].x : trailMarkers[trailMarkerIndex - 1].x,
                            //    nextMaxXValue, destPosition.x, out bool clampedX);
                            float nextMaxYValue = blinkTrailPosition.y + rotatingAnglesForwardVector.y;
                            //float clampedMaxYValue = PartialMathUtil.DirectionMaxValueClamped(trailMarkerIndex == 0 ? StartPositions[i].y : trailMarkers[trailMarkerIndex - 1].y,
                            //    nextMaxYValue, destPosition.y, out bool clampedY);
                            float nextMaxZValue = blinkTrailPosition.z + rotatingAnglesForwardVector.z;
                            //float clampedMaxZValue = PartialMathUtil.DirectionMaxValueClamped(trailMarkerIndex == 0 ? StartPositions[i].z : trailMarkers[trailMarkerIndex - 1].z,
                            //    nextMaxZValue, destPosition.z, out bool clampedZ);

                            float newPositionX = PositionUtil.CalculateClosestMultipleOrClamp(blinkTrailPosition.x, nextMaxXValue,
                                timestepDeltaTime);
                            float newPositionY = PositionUtil.CalculateClosestMultipleOrClamp(blinkTrailPosition.y, nextMaxYValue,
                                timestepDeltaTime);
                            float newPositionZ = PositionUtil.CalculateClosestMultipleOrClamp(blinkTrailPosition.z, nextMaxZValue,
                                timestepDeltaTime);

                            blinkTrailPosition = new Vector3(newPositionX, newPositionY, newPositionZ);

                            //Debug.Log($"{i}: {newPosition}, {trailElapsedDeltaTime}, {clampedMaxXValue}, {clampedMaxYValue}, {clampedMaxZValue}, newMovementRotation: {movementRotation}, {rotatingAnglesForwardVector.x}, {rotatingAnglesForwardVector.y}, {rotatingAnglesForwardVector.z}, {rotatingAnglesForwardVector.magnitude}");
                            //trailPositions[j] = blinkTrailPosition;

                            // calculate the new rotation.

                            // distance is not needed and manhattan can be used but usually the rotation will just use distance instead.

                            //bool trailMarkerIndexCompleted = (clampedX && clampedY && clampedZ) || distance <= 0.1f;
                            //if (!trailMarkerIndexCompleted)
                            if (!destPositionReached)
                            {
                                break;
                            }
                        }
                        
                        //Debug.Log(distance);
                    }
                    if (trailMarkerIndex == trailMarkers.Length)
                    {
                        stopIndex = j;
                        break;
                    }
                }

                int trailPositionsLength = stopIndex + 1;

                Vector3[] trailForwardVectorsNormalized = new Vector3[trailPositionsLength];
                Array.Copy(trailForwardVectorsNormalizedFullSize, trailForwardVectorsNormalized, trailPositionsLength);

                Vector3[] trailRotations = PositionUtil.SmoothenForwardVectors_MovingAverageWindow3_ToRotations(trailForwardVectorsNormalized);

                Vector3[] trailPositions = new Vector3[trailPositionsLength];
                blinkTrailPosition = trailStartPositions[i];
                allTrailPositions[i] = trailPositions;
                for (int j = 0; j < trailPositionsLength; j++) 
                {
                    RotatingCoordinateXYVector3Angles rotatingAngles = new RotatingCoordinateXYVector3Angles(trailRotations[j]);
                    rotatingAnglesForwardVector = rotatingAngles.RotateXY_Forward(!(j == stopIndex && useLastPositionTargetDistance) ? targetDistance : lastPositionTargetDistance);
                    // the final position is difficult to set, sometimes it is past the end position.
                    //Debug.Log($"{rotatingAnglesForwardVector}, {rotatingAnglesForwardVector.magnitude}");
                    float nextMaxXValue = blinkTrailPosition.x + rotatingAnglesForwardVector.x;
                    float nextMaxYValue = blinkTrailPosition.y + rotatingAnglesForwardVector.y;
                    float nextMaxZValue = blinkTrailPosition.z + rotatingAnglesForwardVector.z;

                    float newPositionX = PositionUtil.CalculateClosestMultipleOrClamp(blinkTrailPosition.x, nextMaxXValue,
                        timestepDeltaTime);
                    float newPositionY = PositionUtil.CalculateClosestMultipleOrClamp(blinkTrailPosition.y, nextMaxYValue,
                        timestepDeltaTime);
                    float newPositionZ = PositionUtil.CalculateClosestMultipleOrClamp(blinkTrailPosition.z, nextMaxZValue,
                        timestepDeltaTime);
                    blinkTrailPosition = new Vector3(newPositionX, newPositionY, newPositionZ);
                    trailPositions[j] = blinkTrailPosition;
                }

                PositionUtil.SmoothenPositions_MovingAverageWindow3(trailPositions);
            }

            return allTrailPositions;
        }
        public override void Complete()
        {
            base.Complete();
        }
        public override void CleanUpInstance()
        {
            Trails = null;
            TrailProps = null;
            TrailPositions = null;
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(TrailMoverBuilder_TargetPos))]
    public class TrailMoverBuilder_TargetPosEditor : AbstractEditor<TrailMoverBuilder_TargetPos>
    {
        public bool StartPositionOffsetOverride = false;
        public Vector3 PlayerStartPositionOffset;
        public bool FullEndPositionOverride = false;
        public Vector3 FullEndPosition;
        public bool PropsEndPositionOffsetOverride = false;
        public Vector3 PropsEndPositionOffset;
        public bool PropsIndexOverride = false;
        public int PropsIndex;

        public TrailMoverBuilder_TargetPosEditor_Props Props;

        private bool BakeTrailMesh = false;
        private bool BakeTrailMeshReinitCompleted = false;
        private bool BakeTrailMeshCompleted = false;

        private float TimeRequiredSec = 1f;

        private long StartTime;
        private long LastUpdateTime;

        // Enabled only when a new array has been added in dev mode then turned back off.
        private bool ForceResetPropsFlag = false;
        public void SetOverrides(Vector3 playerStartPositionOffsetOverride, Vector3? fullEndPositionOverride, Vector3 propsEndPositionOffsetOverride, int propsIndex)
        {
            StartPositionOffsetOverride = true;
            PlayerStartPositionOffset = playerStartPositionOffsetOverride;

            if (fullEndPositionOverride != null)
            {
                FullEndPositionOverride = true;
                FullEndPosition = fullEndPositionOverride.Value;
            }
            else
            {
                PropsEndPositionOffsetOverride = true;
                PropsEndPositionOffset = propsEndPositionOffsetOverride;
            }

            PropsIndexOverride = true;
            PropsIndex = propsIndex;
        }
        protected override bool OnInitialize(TrailMoverBuilder_TargetPos instance, ObserverUpdateCache observerUpdateCache)
        {
            TrailMoverBuilder_TargetPosEditor_Props props = Props;

            int propsIndex = PropsIndexOverride ? PropsIndex : Props.PropsIndex;
            if (props != null && props.BlinkRibbonTrailProps != null && propsIndex < props.BlinkRibbonTrailProps.Length)
            {
                SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
                if (system != null)
                {
                    string blinkRibbonTrailRendererType = AbilityFXComponentType.BlinkRibbonTrailRenderer.ToString();
                    BlinkRibbonTrailRenderer blinkRibbonTrailRendererPrefab = (BlinkRibbonTrailRenderer)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == blinkRibbonTrailRendererType);

                    if (blinkRibbonTrailRendererPrefab != null)
                    {
                        float startRotationY = Instance.transform.localEulerAngles.y;
                        float startRotationYCosYAngle = (float)Math.Cos(startRotationY * Mathf.Deg2Rad);
                        float startRotationYSinYAngle = (float)Math.Sin(startRotationY * Mathf.Deg2Rad);

                        BlinkRibbonTrailProps blinkRibbonTrailProps = props.BlinkRibbonTrailProps[propsIndex];
                        if (blinkRibbonTrailProps != null)
                        {
                            BlinkRibbonTrailRenderer[] blinkRibbonTrailRenderers = new BlinkRibbonTrailRenderer[blinkRibbonTrailProps.StartPositionOffsetsLocal.Length];
                            for (int j = 0; j < blinkRibbonTrailRenderers.Length; j++)
                            {
                                blinkRibbonTrailRenderers[j] = GameObject.Instantiate(blinkRibbonTrailRendererPrefab, instance.transform);
                            }

                            Vector3 endPositionWorld = FullEndPositionOverride ? FullEndPosition : (instance.transform.position + 
                                (PropsEndPositionOffsetOverride ? PropsEndPositionOffset : Props.PropsEndPositionOffsets[propsIndex]));

                            if (observerUpdateCache == null)
                            {
                                SetObserverUpdateCache();
                                observerUpdateCache = ObserverUpdateCache;
                            }

                            instance.Initialize(observerUpdateCache, system, PlayerStartPositionOffset, blinkRibbonTrailRenderers, blinkRibbonTrailProps, startRotationY,
                                startRotationYCosYAngle, startRotationYSinYAngle,
                                TimeRequiredSec,
                                endPositionWorld: endPositionWorld);
                            TryAddParticleSystem(instance.gameObject);
                            StartTime = observerUpdateCache.UpdateTickTimeRenderThread;
                            BakeTrailMeshReinitCompleted = true;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Props = (TrailMoverBuilder_TargetPosEditor_Props) EditorGUILayout.ObjectField(Props != null ? Props : GameObject.FindFirstObjectByType<TrailMoverBuilder_TargetPosEditor_Props>(), 
                typeof(TrailMoverBuilder_TargetPosEditor_Props), true);

            if (Props != null)
            {
                Undo.RecordObject(Props, "Props");
                if (GUILayout.Button("Reset Transform Positions"))
                {
                    Transform markersParent = Props.MarkersParent;
                    Transform[] markerTrailsForProps = GetFirstLevelTransforms(markersParent);
                    for (int i = 0; i < TrailEffectsConstants.BlinkRibbonTrailTypeEnumLength; i++)
                    {
                        BlinkRibbonTrailType blinkRibbonTrailType = (BlinkRibbonTrailType)i;
                        if (TrailEffectsConstants.BlinkRibbonTrailProps.TryGetValue(blinkRibbonTrailType, out BlinkRibbonTrailProps props))
                        {
                            Transform markerTrailItemContainer = markerTrailsForProps[i];
                            Transform[] markersForTrailsContainer = GetFirstLevelTransforms(markerTrailItemContainer);
                            for (int j = 0; j < markersForTrailsContainer.Length; j++)
                            {
                                if (j < props.TrailMarkersLocal.Length)
                                {
                                    Transform[] markersForTrail = GetFirstLevelTransforms(markersForTrailsContainer[j]);
                                    for (int k = 0; k < markersForTrail.Length; k++)
                                    {
                                        if (k < props.TrailMarkersLocal[j].Items.Length)
                                        {
                                            markersForTrail[k].transform.localPosition = props.TrailMarkersLocal[j].Items[k];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (GUILayout.Button("Reset Props") || ForceResetPropsFlag)
                {
                    BlinkRibbonTrailProps[] newPropsArray = new BlinkRibbonTrailProps[TrailEffectsConstants.BlinkRibbonTrailProps.Count];
                    int newPropsArrayIndex = 0;
                    for (int i = 0; i < TrailEffectsConstants.BlinkRibbonTrailTypeEnumLength; i++)
                    {
                        BlinkRibbonTrailType blinkRibbonTrailType = (BlinkRibbonTrailType)i;
                        if (TrailEffectsConstants.BlinkRibbonTrailProps.TryGetValue(blinkRibbonTrailType, out BlinkRibbonTrailProps props))
                        {
                            newPropsArray[newPropsArrayIndex++] = props;
                        }
                    }
                    Props.BlinkRibbonTrailProps = newPropsArray;

                    Props.NumProps = newPropsArray.Length;

                    ExpandEndPositions(newPropsArray.Length);

                    ForceResetPropsFlag = false;
                }

                // Does not reset props after changing (intentional for testing different props).
                Props.PropsIndex = EditorGUILayout.IntField("PropsIndex", Props.PropsIndex);
                EditorGUI.BeginChangeCheck();
                Props.NumProps = EditorGUILayout.IntField("NumProps", Props.NumProps);
                int numProps = Props.NumProps;
                Vector3[] propsEndPositionOffsets = Props.PropsEndPositionOffsets;
                for (int i = 0; i < numProps; i++)
                {
                    EditorGUILayout.BeginVertical("GroupBox");
                    EditorGUILayout.LabelField($"PropsIndex {i}");
                    // i think existingProp can be removed now.
                    bool existingProp = i < Props.BlinkRibbonTrailProps.Length && Props.BlinkRibbonTrailProps[i] != null;
                    EditorGUI.BeginChangeCheck();
                    //int numTrails = EditorGUILayout.IntField((existingProp && Props.BlinkRibbonTrailProps[i] != null &&
                    //    Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets != null) ? 
                    //    Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets.Length 
                    //    : 0);
                    int numTrails = EditorGUILayout.IntField("NumTrails", existingProp ? Props.BlinkRibbonTrailProps[i].NumTrails : 0);
                    propsEndPositionOffsets[i] = EditorGUILayout.Vector3Field("PropsEndPositionOffset", propsEndPositionOffsets[i]);
                    float velocity = EditorGUILayout.FloatField("Velocity", existingProp ? Props.BlinkRibbonTrailProps[i].Velocity : 0f);

                    Vector3[] startPositionOffsetsLocal = new Vector3[numTrails];
                    Vector3[] endPositionOffsetsLocal = new Vector3[numTrails];
                    Vector3[] startRotationOffsetsLocal = new Vector3[numTrails];
                    float[] widthMultipliers = new float[numTrails];
                    int[] allTrailsNumTrailMarkers = new int[numTrails];
                    SerializeableArray<Vector3>[] allTrailsTrailMarkersLocal = new SerializeableArray<Vector3>[numTrails];
                    //bool numTrailsChanged = EditorGUI.EndChangeCheck();

                    //EditorGUI.BeginChangeCheck();
                    for (int j = 0; j < numTrails; j++)
                    {
                        EditorGUILayout.BeginVertical("GroupBox");
                        EditorGUILayout.LabelField($"PropsIndex {i}, TrailIndex {j}");
                        allTrailsNumTrailMarkers[j] = EditorGUILayout.IntField("NumTrailMarkers", (existingProp && Props.BlinkRibbonTrailProps[i].NumTrailMarkers != null) ?
                            Props.BlinkRibbonTrailProps[i].NumTrailMarkers[j] : 0);
                        Vector3[] trailMarkersLocal = new Vector3[allTrailsNumTrailMarkers[j]];
                        allTrailsTrailMarkersLocal[j] = new SerializeableArray<Vector3>(trailMarkersLocal);
                        startPositionOffsetsLocal[j] = CreateEditorField("StartPositionOffsetLocal", existingProp ? Props.BlinkRibbonTrailProps[i].StartPositionOffsetsLocal : null, j);
                        endPositionOffsetsLocal[j] = CreateEditorField("EndPositionOffsetLocal", existingProp ? Props.BlinkRibbonTrailProps[i].EndPositionOffsetsLocal : null, j);
                        startRotationOffsetsLocal[j] = CreateEditorField("StartRotationOffsetLocal", existingProp ? Props.BlinkRibbonTrailProps[i].StartRotationOffsetsLocal : null, j);
                        widthMultipliers[j] = CreateEditorField("WidthMultiplier", existingProp ? Props.BlinkRibbonTrailProps[i].WidthMultipliers : null, j);

                        Transform markersParent = Props.MarkersParent;
                        if (markersParent != null)
                        {
                            Transform[] markerTrailsForProps = GetFirstLevelTransforms(markersParent);

                            if (i < markerTrailsForProps.Length)
                            {
                                // __TrailMarkers
                                Transform markerTrailItemContainer = markerTrailsForProps[i];
                                for (int k = 0; k < allTrailsNumTrailMarkers[j]; k++)
                                {
                                    // __TrailX
                                    Transform[] markersForTrailsContainer = GetFirstLevelTransforms(markerTrailItemContainer);

                                    Transform[] markersForTrail = j < markersForTrailsContainer.Length ? GetFirstLevelTransforms(markersForTrailsContainer[j]) : null;
                                    EditorGUILayout.ObjectField($"MarkerTransform_Trail{j}_Marker{k}",
                                        (markersForTrail != null && k < markersForTrail.Length) ? markersForTrail[k] : null,
                                        typeof(Transform), true);
                                    //Debug.Log($"{j < Props.BlinkRibbonTrailProps[i].TrailMarkersLocal.Length}, {k < Props.BlinkRibbonTrailProps[i].TrailMarkersLocal[j].Length}"); 
                                    if (existingProp && Props.BlinkRibbonTrailProps[i].TrailMarkersLocal != null && 
                                        j < Props.BlinkRibbonTrailProps[i].TrailMarkersLocal.Length &&
                                        k < Props.BlinkRibbonTrailProps[i].TrailMarkersLocal[j].Items.Length)
                                    {
                                        Vector3 markerPosition = k < markersForTrail.Length ? markersForTrail[k].localPosition : Vector3.zero;
                                        Props.BlinkRibbonTrailProps[i].TrailMarkersLocal[j].Items[k] = markerPosition;
                                    }
                                    // Readonly vector3.
                                    trailMarkersLocal[k] = CreateEditorField($"TrailMarkerPos_Trail{j}_Marker", 
                                        (existingProp && j < Props.BlinkRibbonTrailProps[i].TrailMarkersLocal.Length) ? Props.BlinkRibbonTrailProps[i].TrailMarkersLocal[j].Items : null, k);
                                }
                            }
                            
                            
                        }
                        EditorGUILayout.EndVertical();
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        BlinkRibbonTrailProps blinkRibbonTrailProps = new BlinkRibbonTrailProps(
                            velocity: velocity,
                            numTrails: numTrails,
                            startPositionOffsetsLocal: startPositionOffsetsLocal, endPositionOffsetsLocal: endPositionOffsetsLocal, startRotationOffsetsLocal: startRotationOffsetsLocal,
                            widthMultipliers: widthMultipliers, numTrailMarkers: allTrailsNumTrailMarkers, trailMarkersLocal: allTrailsTrailMarkersLocal);
                        Props.BlinkRibbonTrailProps[i] = blinkRibbonTrailProps;
                        EditorDestroy();
                    }
                    EditorGUILayout.EndVertical();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    BlinkRibbonTrailProps[] blinkRibbonTrailProps = new BlinkRibbonTrailProps[numProps];
                    if (Props.BlinkRibbonTrailProps != null)
                    {
                        Array.Copy(Props.BlinkRibbonTrailProps, blinkRibbonTrailProps, Props.BlinkRibbonTrailProps.Length > numProps ? numProps : Props.BlinkRibbonTrailProps.Length);
                    }
                    Props.BlinkRibbonTrailProps = blinkRibbonTrailProps;

                    Props.PropsEndPositionOffsets = propsEndPositionOffsets;
                    EditorDestroy();
                }

                EditorGUI.BeginChangeCheck();
                BakeTrailMesh = GUILayout.Toggle(BakeTrailMesh, "Bake Trail Mesh");
                if (EditorGUI.EndChangeCheck())
                {
                    BakeTrailMeshReinitCompleted = false;
                    EditorDestroy();
                }
            }

        }

        private void ExpandEndPositions(int propsLength)
        {
            Vector3[] newEndPositions = new Vector3[propsLength];
            Vector3[] previousEndPositions = Props.PropsEndPositionOffsets;
            Array.Copy(previousEndPositions, newEndPositions, Math.Min(newEndPositions.Length, previousEndPositions.Length));
            Props.PropsEndPositionOffsets = newEndPositions;
        }

        protected override void ManualUpdate()
        {
            Instance.ManualUpdate();

            TrySaveBakedTrailMesh();

            LastUpdateTime = ObserverUpdateCache.UpdateTickTimeRenderThread;
        }
        protected void TrySaveBakedTrailMesh()
        {
            if (BakeTrailMesh && Instance.Completed && BakeTrailMeshReinitCompleted && !BakeTrailMeshCompleted)
            {
                // complete it first so when the file name is being inquired the manual update does not open another panel.
                BakeTrailMeshCompleted = true;
                string trailName = EditorUtility.SaveFilePanelInProject("Save trails as mesh prefix", $"Trail_TargetPos_{Props.PropsIndex}", "", "", "Assets/Crafter/Components/VFX/Trails/Meshes");

                if (trailName.Length > 0)
                {
                    var trails = Instance.Trails;

                    for (int i = 0; i < trails.Length; i++)
                    {
                        BlinkRibbonTrailRenderer trail = trails[i];
                        Mesh trailMesh = new Mesh();
                        trail.TrailRenderer.BakeMesh(trailMesh, useTransform: false);
                        AssetDatabase.CreateAsset(trailMesh, $"{trailName}_{i}.asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                }
            }
        }

        protected override void EditorDestroy()
        {
            StartTime = default;
            var trails = Instance.Trails;
            if (trails != null)
            {
                for (int i = 0; i < trails.Length; i++)
                {
                    GameObject.DestroyImmediate(trails[i].gameObject);
                }
            }
            BakeTrailMeshCompleted = false;
            Instance.CleanUpInstance();
        }
    }

#endif

}
