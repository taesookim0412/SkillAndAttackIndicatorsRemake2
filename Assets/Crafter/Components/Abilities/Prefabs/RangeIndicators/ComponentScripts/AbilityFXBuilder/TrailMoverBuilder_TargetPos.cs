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
        [HideInInspector]
        private BlinkRibbonTrailProps TrailProps;

        [HideInInspector]
        private Vector3 StartPosition;
        [HideInInspector]
        private Vector3 EndPositionTracker;
        [HideInInspector]
        private Vector3[] MovementRotations;
        [HideInInspector]
        private float TimeRequiredSec;
        [HideInInspector]
        private float TimeRequiredSecReciprocal;
        [HideInInspector]
        private Vector3[][] TrailMarkersWorldAndEndPosition;
        [HideInInspector]
        private int[] TargetMarkerPositionIndices;

        [HideInInspector]
        private float ElapsedTimeSec;
        [HideInInspector]
        private Vector3[] RotatingAnglesForwardVectors;

        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache,
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
            StartPosition = startWorldPosition;

            float[] widthMultipliers = blinkRibbonTrailProps.WidthMultipliers; 
            Vector3[] localStartPositionOffsets = blinkRibbonTrailProps.StartPositionOffsetsLocal;
            for (int i = 0; i < trails.Length; i++)
            {
                Vector3 startPositionOffset = localStartPositionOffsets[i];
                float startPositionOffsetX = startPositionOffset.x;
                float startPositionOffsetZ = startPositionOffset.z;

                float rotatedLocalPositionX = startPositionOffsetZ * startRotationYSinYAngle + startPositionOffsetX * startRotationYCosYAngle;
                float rotatedLocalPositionZ = startPositionOffsetZ * startRotationYCosYAngle - startPositionOffsetX * startRotationYSinYAngle;

                BlinkRibbonTrailRenderer trail = trails[i];
                trail.SetTrailRendererWidth(widthMultipliers[i]);
                trail.transform.position = new Vector3(startWorldPosition.x + rotatedLocalPositionX,
                    startWorldPosition.y + startPositionOffset.y,
                    startWorldPosition.z + rotatedLocalPositionZ);
            }

            Vector3[] localStartRotationOffsets = blinkRibbonTrailProps.StartRotationOffsetsLocal;
            Vector3[] movementRotations = new Vector3[trails.Length];
            for (int i = 0; i < trails.Length; i++)
            {
                Vector3 startRotation = localStartRotationOffsets[i].Copy();
                startRotation.y += startRotationY;
                movementRotations[i] = startRotation;
            }
            MovementRotations = movementRotations;

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

                    trailMarkersWorldAndEndPosition[j] = new Vector3(startWorldPosition.x + rotatedPositionOffsetX,
                        startWorldPosition.y + positionOffset.y,
                        startWorldPosition.z + rotatedPositionOffsetZ);
                }

                // end position is not set yet.
                allTrailMarkersWorldAndEndPosition[i] = trailMarkersWorldAndEndPosition;
            }

            TrailMarkersWorldAndEndPosition = allTrailMarkersWorldAndEndPosition;

            SetEndPositionsWorld(startRotationY, endPositionWorld, startRotationYCosYAngle, startRotationYSinYAngle, allTrailMarkersWorldAndEndPosition);

            TimeRequiredSec = timeRequiredSec;
            TimeRequiredSecReciprocal = 1 / timeRequiredSec;

            TargetMarkerPositionIndices = new int[trails.Length];

            ElapsedTimeSec = 0f;

            Vector3[] rotatingAnglesForwardVectors = new Vector3[trails.Length];
            for (int i = 0; i < rotatingAnglesForwardVectors.Length; i++)
            {
                // could be rotated.
                rotatingAnglesForwardVectors[i] = Vector3.forward;
            }
            RotatingAnglesForwardVectors = rotatingAnglesForwardVectors;
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

            EndPositionTracker = newEndPositionWorld;
        }

        public void ManualUpdate()
        {
            float elapsedDeltaTime = ObserverUpdateCache.UpdateTickTimeFixedUpdateDeltaTimeSec;
            float elapsedTimeSec = ElapsedTimeSec;
            if (elapsedTimeSec < TimeRequiredSec)
            {
                MoveTrailToEndPositions(elapsedDeltaTime,
                    TimeRequiredSecReciprocal,
                    Trails, TrailMarkersWorldAndEndPosition,
                    TargetMarkerPositionIndices,
                    MovementRotations,
                    RotatingAnglesForwardVectors
                    );

            }

            ElapsedTimeSec += elapsedDeltaTime;
        }

        public void MoveTrailToEndPositions(float elapsedDeltaTime,
            float timeRequiredSecReciprocal,
            BlinkRibbonTrailRenderer[] blinkTrails,
            Vector3[][] trailMarkersWorldAndEndPosition,
            int[] targetMarkerPositionIndices,
            Vector3[] movementRotations,
            Vector3[] rotatingAnglesForwardVectors)
        {
            for (int i = 0; i < blinkTrails.Length; i++)
            {
                float trailElapsedDeltaTime = elapsedDeltaTime;
                //float trailElapsedTimeSec = elapsedTimeSec;

                int trailMarkerIndex = targetMarkerPositionIndices[i];
                Vector3[] trailMarkers = trailMarkersWorldAndEndPosition[i];
                if (trailMarkerIndex < trailMarkers.Length)
                {
                    for (; trailMarkerIndex < trailMarkers.Length; trailMarkerIndex++)
                    {
                        Transform blinkTrailTransform = blinkTrails[i].transform;

                        Vector3 currentPosition = blinkTrailTransform.position;
                        Vector3 destPosition = trailMarkers[trailMarkerIndex];
                        Vector3 movementRotation = movementRotations[i];
                        //Debug.Log($"{i}: {currentPosition}, {destPosition}, previousMovementRotation: {movementRotation}");
                        Vector3 rotatingAnglesForwardVector = rotatingAnglesForwardVectors[i];


                        Vector3 directionVector = destPosition - currentPosition;

                        float distance = directionVector.magnitude;

                        float dtxTargetVelocity = trailElapsedDeltaTime * 50f;
                        Vector3 rotation = Vector3Util.LookRotationPitchYaw(directionVector);

                        //float timeElapsedPercentage = trailElapsedTimeSec * timeRequiredSecReciprocal;
                        //float easeTimePercentage = EffectsUtil.EaseInOutQuad(timeElapsedPercentage);
                        
                        float rotationXDifference = PartialMathUtil.DeltaAngle(movementRotation.x, rotation.x);
                        float rotationYDifference = PartialMathUtil.DeltaAngle(movementRotation.y, rotation.y);

                        movementRotation.x = PartialMathUtil.LerpDeltaAngle(movementRotation.x, rotationXDifference, dtxTargetVelocity);
                        //float maxMovementRotationX = PartialMathUtil.LerpDeltaAngle(movementRotation.x, rotationXDifference, trailElapsedTimeSec * rotationSpeedMultiplier);
                        //movementRotation.x = PositionUtil.CalculateClosestMultipleOrClamp(movementRotation.x, maxMovementRotationX, elapsedDeltaTime);
                        //movementRotation.x = movementRotation.x + rotationXDifference * easeTimePercentage;

                        movementRotation.y = PartialMathUtil.LerpDeltaAngle(movementRotation.y, rotationYDifference, dtxTargetVelocity);
                        //float maxMovementRotationY = PartialMathUtil.LerpDeltaAngle(movementRotation.y, rotationYDifference, trailElapsedTimeSec * rotationSpeedMultiplier);
                        //movementRotation.y = PositionUtil.CalculateClosestMultipleOrClamp(movementRotation.y, maxMovementRotationY, elapsedDeltaTime);
                        //movementRotation.y = movementRotation.y + rotationYDifference * easeTimePercentage;

                        RotatingCoordinateVector3Angles rotatingAngles = new RotatingCoordinateVector3Angles(movementRotation);
                        rotatingAnglesForwardVector = rotatingAngles.RotateXY_Forward(distance);
                        //Debug.Log($"{i}: {movementRotation}, {directionVector}, {endPosition}, {currentPosition}");

                        rotatingAnglesForwardVectors[i] = rotatingAnglesForwardVector;
                        // chances are both x and y need to change so its better to set all at once.
                        movementRotations[i] = movementRotation;

                        // this will be short because the total distance is not curved so it needs to be faster ( mult by 5 ).
                        //float targetVelocity = totalDirectionDistance * timeRequiredSecReciprocal * 5f;
                        //float dtxTargetVelocity = trailElapsedDeltaTime * targetVelocity;

                        float nextMaxXValue = currentPosition.x + rotatingAnglesForwardVector.x * dtxTargetVelocity;
                        float clampedMaxXValue = PartialMathUtil.DirectionMaxValueClamped(trailMarkerIndex == 0 ? StartPosition.x : trailMarkers[trailMarkerIndex - 1].x,
                            nextMaxXValue, destPosition.x, out bool clampedX);
                        float nextMaxYValue = currentPosition.y + rotatingAnglesForwardVector.y * dtxTargetVelocity;
                        float clampedMaxYValue = PartialMathUtil.DirectionMaxValueClamped(trailMarkerIndex == 0 ? StartPosition.y : trailMarkers[trailMarkerIndex - 1].y,
                            nextMaxYValue, destPosition.y, out bool clampedY);
                        float nextMaxZValue = currentPosition.z + rotatingAnglesForwardVector.z * dtxTargetVelocity;
                        float clampedMaxZValue = PartialMathUtil.DirectionMaxValueClamped(trailMarkerIndex == 0 ? StartPosition.z : trailMarkers[trailMarkerIndex - 1].z,
                            nextMaxZValue, destPosition.z, out bool clampedZ);

                        float newPositionX = PositionUtil.CalculateClosestMultipleOrClamp(currentPosition.x, clampedMaxXValue,
                            trailElapsedDeltaTime);
                        float newPositionY = PositionUtil.CalculateClosestMultipleOrClamp(currentPosition.y, clampedMaxYValue,
                            trailElapsedDeltaTime);
                        float newPositionZ = PositionUtil.CalculateClosestMultipleOrClamp(currentPosition.z, clampedMaxZValue,
                            trailElapsedDeltaTime);

                        Vector3 newPosition = new Vector3(newPositionX, newPositionY, newPositionZ);

                        //Debug.Log($"{i}: {newPosition}, {trailElapsedDeltaTime}, {clampedMaxXValue}, {clampedMaxYValue}, {clampedMaxZValue}, newMovementRotation: {movementRotation}, {rotatingAnglesForwardVector.x}, {rotatingAnglesForwardVector.y}, {rotatingAnglesForwardVector.z}, {rotatingAnglesForwardVector.magnitude}");
                        blinkTrailTransform.position = newPosition;

                        // calculate the new rotation.

                        // distance is not needed and manhattan can be used but usually the rotation will just use distance instead.

                        bool trailMarkerIndexCompleted = (clampedX && clampedY && clampedZ) || distance <= 0.1f;
                        if (!trailMarkerIndexCompleted)
                        {
                            break;

                        }
                        else
                        {
                            // dp = v * dt
                            // dt = dp / v
                            float velocityReciprocal = 1 / 50f;
                            float xDeltaTime = (float)Math.Abs(clampedMaxXValue - currentPosition.x) * velocityReciprocal;
                            float yDeltaTime = (float)Math.Abs(clampedMaxYValue - currentPosition.y) * velocityReciprocal;
                            float zDeltaTime = (float)Math.Abs(clampedMaxZValue - currentPosition.z) * velocityReciprocal;
                            float maxDeltaTimeElapsed = (float)Math.Max(xDeltaTime, Math.Max(yDeltaTime, zDeltaTime));

                            trailElapsedDeltaTime -= maxDeltaTimeElapsed;
                            //trailElapsedDeltaTime = 0f;
                        }


                        //Debug.Log(distance);
                    }

                    targetMarkerPositionIndices[i] = trailMarkerIndex;
                }


            }
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(TrailMoverBuilder_TargetPos))]
    public class TrailMoverBuilder_TargetPosEditor : AbstractEditor<TrailMoverBuilder_TargetPos>
    {
        private TrailMoverBuilder_TargetPosEditor_Props Props;

        private float TimeRequiredSec = 0.5f;

        private long StartTime;
        private long LastUpdateTime;
        
        protected override bool OnInitialize(TrailMoverBuilder_TargetPos instance)
        {
            TrailMoverBuilder_TargetPosEditor_Props props = Props;
            if (props != null && props.BlinkRibbonTrailProps != null && props.PropsIndex < props.BlinkRibbonTrailProps.Length)
            {
                SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
                if (system != null)
                {
                    SetObserverUpdateCache();

                    string blinkRibbonTrailRendererType = AbilityFXComponentType.BlinkRibbonTrailRenderer.ToString();
                    BlinkRibbonTrailRenderer blinkRibbonTrailRendererPrefab = (BlinkRibbonTrailRenderer)system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == blinkRibbonTrailRendererType);

                    if (blinkRibbonTrailRendererPrefab != null)
                    {
                        float startRotationY = Instance.transform.localEulerAngles.y;
                        float startRotationYCosYAngle = (float)Math.Cos(startRotationY * Mathf.Deg2Rad);
                        float startRotationYSinYAngle = (float)Math.Sin(startRotationY * Mathf.Deg2Rad);

                        BlinkRibbonTrailProps blinkRibbonTrailProps = props.BlinkRibbonTrailProps[props.PropsIndex];
                        if (blinkRibbonTrailProps != null)
                        {
                            BlinkRibbonTrailRenderer[] blinkRibbonTrailRenderers = new BlinkRibbonTrailRenderer[blinkRibbonTrailProps.StartPositionOffsetsLocal.Length];
                            for (int j = 0; j < blinkRibbonTrailRenderers.Length; j++)
                            {
                                blinkRibbonTrailRenderers[j] = GameObject.Instantiate(blinkRibbonTrailRendererPrefab, instance.transform);
                            }
                            Vector3 endPositionWorld = Props.EndPositionLocal + instance.transform.position;

                            instance.Initialize(ObserverUpdateCache, blinkRibbonTrailRenderers, blinkRibbonTrailProps, startRotationY,
                                startRotationYCosYAngle, startRotationYSinYAngle,
                                TimeRequiredSec,
                                endPositionWorld: endPositionWorld);
                            TryAddParticleSystem(instance.gameObject);
                            StartTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
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
                if (GUILayout.Button("Reset Props"))
                {
                    BlinkRibbonTrailProps[] newPropsArray = new BlinkRibbonTrailProps[TrailEffectsConstants.BlinkRibbonTrailProps.Count];
                    int newPropsArrayIndex = 0;
                    for (int i = 0; i < TrailEffectsConstants.BlinkRibbonTrailTypeEnumLength; i++)
                    {
                        BlinkRibbonTrailType blinkRibbonTrailType = (BlinkRibbonTrailType)i;
                        if (TrailEffectsConstants.BlinkRibbonTrailProps.TryGetValue(blinkRibbonTrailType, out BlinkRibbonTrailProps props)) {
                            newPropsArray[newPropsArrayIndex++] = props;
                        }
                    }
                    Props.BlinkRibbonTrailProps = newPropsArray;

                    Props.NumProps = newPropsArray.Length;
                }

                Props.EndPositionLocal = EditorGUILayout.Vector3Field("LocalEndPosition", Props.EndPositionLocal);
                Props.PropsIndex = EditorGUILayout.IntField("PropsIndex", Props.PropsIndex);
                EditorGUI.BeginChangeCheck();
                Props.NumProps = EditorGUILayout.IntField("NumProps", Props.NumProps);
                int numProps = Props.NumProps;
                for (int i = 0; i < numProps; i++)
                {
                    // i think existingProp can be removed now.
                    bool existingProp = i < Props.BlinkRibbonTrailProps.Length && Props.BlinkRibbonTrailProps[i] != null;
                    EditorGUI.BeginChangeCheck();
                    //int numTrails = EditorGUILayout.IntField((existingProp && Props.BlinkRibbonTrailProps[i] != null &&
                    //    Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets != null) ? 
                    //    Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets.Length 
                    //    : 0);
                    int numTrails = EditorGUILayout.IntField("NumTrails", existingProp ? Props.BlinkRibbonTrailProps[i].NumTrails : 0);
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
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        BlinkRibbonTrailProps blinkRibbonTrailProps = new BlinkRibbonTrailProps(numTrails: numTrails,
                            startPositionOffsetsLocal: startPositionOffsetsLocal, endPositionOffsetsLocal: endPositionOffsetsLocal, startRotationOffsetsLocal: startRotationOffsetsLocal,
                            widthMultipliers: widthMultipliers, numTrailMarkers: allTrailsNumTrailMarkers, trailMarkersLocal: allTrailsTrailMarkersLocal);
                        Props.BlinkRibbonTrailProps[i] = blinkRibbonTrailProps;
                        EditorDestroy();
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    BlinkRibbonTrailProps[] blinkRibbonTrailProps = new BlinkRibbonTrailProps[numProps];
                    if (Props.BlinkRibbonTrailProps != null)
                    {
                        Array.Copy(Props.BlinkRibbonTrailProps, blinkRibbonTrailProps, Props.BlinkRibbonTrailProps.Length > numProps ? numProps : Props.BlinkRibbonTrailProps.Length);
                    }
                    Props.BlinkRibbonTrailProps = blinkRibbonTrailProps;
                    EditorDestroy();
                }
            }

        }

        protected override void ManualUpdate()
        {
            Instance.ManualUpdate();

            LastUpdateTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
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
                Instance.Trails = null;
            }
            //if (LastUpdateTime > StartTime)
            //{
            //    Instance.transform.position = StartPosition;
            //}
        }
    }

#endif

}
