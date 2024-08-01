using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFX;
using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Models;
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

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class TrailMoverBuilder_TargetPos : AbstractAbilityFXBuilder
    {
        [NonSerialized]
        public BlinkRibbonTrailRenderer[] Trails;
        [HideInInspector]
        BlinkRibbonTrailProps TrailProps;

        [HideInInspector]
        private Vector3 EndPositionTracker;
        [HideInInspector]
        private Vector3[] EndPositionsWorld;
        [HideInInspector]
        private Vector3[] MovementRotations;
        [HideInInspector]
        private float TimeRequiredSec;
        [HideInInspector]
        private float TimeRequiredSecReciprocal;

        [HideInInspector]
        private float ElapsedTimeSec;
        [HideInInspector]
        private Vector3 RotatingAnglesForwardVector;

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
            Vector3 endPosition)
        {
            base.Initialize(observerUpdateCache);

            Vector3 startWorldPosition = transform.position;

            Vector3[] localStartPositionOffsets = blinkRibbonTrailProps.LocalStartPositionOffsets;
            for (int i = 0; i < trails.Length; i++)
            {
                Vector3 startPositionOffset = localStartPositionOffsets[i];
                float startPositionOffsetX = startPositionOffset.x;
                float startPositionOffsetZ = startPositionOffset.z;

                float rotatedLocalPositionX = startPositionOffsetZ * startRotationYSinYAngle + startPositionOffsetX * startRotationYCosYAngle;
                float rotatedLocalPositionZ = startPositionOffsetZ * startRotationYCosYAngle - startPositionOffsetX * startRotationYSinYAngle;

                trails[i].transform.position = new Vector3(startWorldPosition.x + rotatedLocalPositionX,
                    startWorldPosition.y,
                    startWorldPosition.z + rotatedLocalPositionZ);
            }

            // Careful, this copies the struct by value then adds to y.
            Vector3[] localStartRotationOffsets = blinkRibbonTrailProps.LocalStartRotationOffsets;
            Vector3[] movementRotations = new Vector3[trails.Length];
            for (int i = 0; i < trails.Length; i++)
            {
                Vector3 startRotation = localStartRotationOffsets[i];
                startRotation.y += startRotationY;
                movementRotations[i] = startRotation;
            }

            MovementRotations = movementRotations;

            Trails = trails;
            TrailProps = blinkRibbonTrailProps;

            SetEndPositionsWorld(startRotationY, endPosition, startRotationYCosYAngle, startRotationYSinYAngle);

            TimeRequiredSec = timeRequiredSec;
            TimeRequiredSecReciprocal = 1 / timeRequiredSec;

            ElapsedTimeSec = 0f;
            RotatingAnglesForwardVector = Vector3.forward;
        }

        public void SetEndPositionsWorld(float startYRotation, Vector3 newEndPosition,
            float startRotationYCosYAngle,
            float startRotationYSinYAngle)
        {
            Vector3[] localEndPositionOffsets = TrailProps.LocalEndPositionOffsets;
            Vector3[] endPositions = new Vector3[localEndPositionOffsets.Length];

            for (int i = 0; i < endPositions.Length; i++)
            {
                Vector3 endPositionOffset = localEndPositionOffsets[i];
                float endPositionOffsetX = endPositionOffset.x;
                float endPositionOffsetZ = endPositionOffset.z;

                float rotatedEndPositionX = endPositionOffsetZ * startRotationYSinYAngle + endPositionOffsetX * startRotationYCosYAngle;
                float rotatedEndPositionZ = endPositionOffsetZ * startRotationYCosYAngle - endPositionOffsetX * startRotationYSinYAngle;
                endPositions[i] = new Vector3(newEndPosition.x + rotatedEndPositionX, newEndPosition.y, newEndPosition.z + rotatedEndPositionZ);
            }

            EndPositionTracker = newEndPosition;
            EndPositionsWorld = endPositions;
        }

        public void ManualUpdate()
        {
            float elapsedTimeSec = ElapsedTimeSec;
            float elapsedDeltaTime = ObserverUpdateCache.UpdateTickTimeFixedUpdateDeltaTimeSec;
            if (elapsedTimeSec < TimeRequiredSec)
            {
                BlinkRibbonTrailRenderer[] blinkTrails = Trails;
                Vector3[] endPositions = EndPositionsWorld;
                Vector3[] movementRotations = MovementRotations;
                for (int i = 0; i < blinkTrails.Length; i++)
                {
                    Vector3 currentPosition = blinkTrails[i].transform.position;
                    Vector3 endPosition = endPositions[i];
                    Vector3 directionVector = endPosition - currentPosition;
                    float distance = directionVector.magnitude;

                    if (distance > 0.05f)
                    {
                        Vector3 movementRotation = movementRotations[i];
                        Vector3 rotatingAnglesForwardVector = RotatingAnglesForwardVector;
                        Vector3 rotation = Vector3Util.LookRotationPitchYaw(directionVector);

                        float timeElapsedPercentage = elapsedTimeSec * TimeRequiredSecReciprocal;
                        float easeTimePercentage = EffectsUtil.EaseInOutQuad(timeElapsedPercentage);

                        float rotationXDifference = PartialMathUtil.DeltaAngle(movementRotation.x, rotation.x);
                        float rotationYDifference = PartialMathUtil.DeltaAngle(movementRotation.y, rotation.y);

                        bool useNewRotationX = rotationXDifference < -3f || rotationXDifference > 3f;
                        bool useNewRotationY = rotationYDifference < -3f || rotationYDifference > 3f;
                        if (useNewRotationX || useNewRotationY)
                        {
                            if (useNewRotationX)
                            {
                                movementRotation.x = movementRotation.x + rotationXDifference * easeTimePercentage;
                            }
                            if (useNewRotationY)
                            {
                                movementRotation.y = movementRotation.y + rotationYDifference * easeTimePercentage;
                            }
                            RotatingCoordinateVector3Angles rotatingAngles = new RotatingCoordinateVector3Angles(movementRotation);
                            rotatingAnglesForwardVector = rotatingAngles.RotateXY_Forward();
                            Debug.Log(movementRotation);

                            RotatingAnglesForwardVector = rotatingAnglesForwardVector;
                            // chances are both x and y need to change so its better to set all at once.
                            movementRotations[i] = movementRotation;
                        }

                        float targetVelocity = distance * TimeRequiredSecReciprocal * 5f;
                        float dtxTargetVelocity = elapsedDeltaTime * targetVelocity;
                        //float dtxTargetVelocity = elapsedDeltaTime;

                        float newPositionX = PositionUtil.CalculateClosestMultipleOrClamp(currentPosition.x, currentPosition.x + rotatingAnglesForwardVector.x * dtxTargetVelocity, elapsedDeltaTime);
                        float newPositionY = PositionUtil.CalculateClosestMultipleOrClamp(currentPosition.y, currentPosition.y + rotatingAnglesForwardVector.y * dtxTargetVelocity, elapsedDeltaTime);
                        float newPositionZ = PositionUtil.CalculateClosestMultipleOrClamp(currentPosition.z, currentPosition.z + rotatingAnglesForwardVector.z * dtxTargetVelocity, elapsedDeltaTime);

                        transform.position = new Vector3(newPositionX, newPositionY, newPositionZ);
                        Debug.Log(distance);
                    }
                }
            }

            ElapsedTimeSec += elapsedDeltaTime;
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
                            BlinkRibbonTrailRenderer[] blinkRibbonTrailRenderers = new BlinkRibbonTrailRenderer[blinkRibbonTrailProps.LocalStartPositionOffsets.Length];
                            for (int j = 0; j < blinkRibbonTrailRenderers.Length; j++)
                            {
                                blinkRibbonTrailRenderers[props.PropsIndex] = GameObject.Instantiate(blinkRibbonTrailRendererPrefab, instance.transform);
                            }

                            instance.Initialize(ObserverUpdateCache, blinkRibbonTrailRenderers, blinkRibbonTrailProps, startRotationY,
                                startRotationYCosYAngle, startRotationYSinYAngle,
                                TimeRequiredSec,
                                endPosition: new Vector3(1f, 2f, 1f));
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
            EditorGUI.BeginChangeCheck();
            Props = (TrailMoverBuilder_TargetPosEditor_Props) EditorGUILayout.ObjectField(Props != null ? Props : GameObject.FindFirstObjectByType<TrailMoverBuilder_TargetPosEditor_Props>(), 
                typeof(TrailMoverBuilder_TargetPosEditor_Props), true);

            if ((EditorGUI.EndChangeCheck() && Props != null) || Props != null)
            {
                Props.PropsIndex = EditorGUILayout.IntField("PropsIndex", Props.PropsIndex);
                EditorGUI.BeginChangeCheck();
                Props.NumProps = EditorGUILayout.IntField("NumProps", Props.NumProps);
                int numProps = Props.NumProps;
                for (int i = 0; i < numProps; i++)
                {
                    bool existingProp = i < Props.BlinkRibbonTrailProps.Length && Props.BlinkRibbonTrailProps[i] != null;
                    EditorGUI.BeginChangeCheck();
                    //int numTrails = EditorGUILayout.IntField((existingProp && Props.BlinkRibbonTrailProps[i] != null &&
                    //    Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets != null) ? 
                    //    Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets.Length 
                    //    : 0);
                    int numTrails = EditorGUILayout.IntField(existingProp ? Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets.Length : 0);
                    Vector3[] localStartPositionOffsets = new Vector3[numTrails];
                    Vector3[] localEndPositionOffsets = new Vector3[numTrails];
                    Vector3[] localStartRotationOffsets = new Vector3[numTrails];
                    //bool numTrailsChanged = EditorGUI.EndChangeCheck();

                    //EditorGUI.BeginChangeCheck();
                    for (int j = 0; j < numTrails; j++)
                    {
                        localStartPositionOffsets[j] = CreateEditorLayout(existingProp ? Props.BlinkRibbonTrailProps[i].LocalStartPositionOffsets : null, j, "LocalStartPositionOffset");
                        localEndPositionOffsets[j] = CreateEditorLayout(existingProp ? Props.BlinkRibbonTrailProps[i].LocalEndPositionOffsets : null, j, "LocalEndPositionOffset");
                        localStartRotationOffsets[j] = CreateEditorLayout(existingProp ? Props.BlinkRibbonTrailProps[i].LocalStartRotationOffsets : null, j, "LocalStartRotationOffset");
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        for (int j = 0; j < numProps; j++)
                        {
                            BlinkRibbonTrailProps blinkRibbonTrailProps = new BlinkRibbonTrailProps(localStartPositionOffsets, localEndPositionOffsets, localStartRotationOffsets);
                            Props.BlinkRibbonTrailProps[i] = blinkRibbonTrailProps;
                        }
                        EditorDestroy();
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Props.NumProps = numProps;
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
