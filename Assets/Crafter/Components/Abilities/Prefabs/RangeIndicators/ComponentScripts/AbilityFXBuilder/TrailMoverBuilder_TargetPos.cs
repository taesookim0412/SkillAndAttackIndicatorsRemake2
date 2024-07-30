using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Models;
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
        public ElectricTrail ElectricTrail;

        [HideInInspector]
        private Vector3 StartPosition;
        [HideInInspector]
        private Vector3 EndPosition;
        [HideInInspector]
        private Vector3 MovementRotation;
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
            ElectricTrail electricTrail,
            Vector3 startPosition, Vector3 endPosition, 
            Vector3 startRotation,
            float timeRequiredSec)
        {
            base.Initialize(observerUpdateCache);

            electricTrail.transform.localPosition = Vector3.zero;
            ElectricTrail = electricTrail;

            StartPosition = startPosition;
            EndPosition = endPosition;
            MovementRotation = startRotation;

            TimeRequiredSec = timeRequiredSec;
            TimeRequiredSecReciprocal = 1 / timeRequiredSec;

            ElapsedTimeSec = 0f;
            RotatingAnglesForwardVector = Vector3.forward;
        }

        public void ManualUpdate()
        {
            float elapsedTimeSec = ElapsedTimeSec;
            float elapsedDeltaTime = ObserverUpdateCache.UpdateTickTimeFixedUpdateDeltaTimeSec;
            if (elapsedTimeSec < TimeRequiredSec)
            {
                Vector3 currentPosition = transform.position;
                Vector3 endPosition = EndPosition;

                float distance = (endPosition - currentPosition).magnitude;

                if (distance > 0.05f)
                {
                    Vector3 movementRotation = MovementRotation;
                    Vector3 rotatingAnglesForwardVector = RotatingAnglesForwardVector;
                    Vector3 rotation = Vector3Util.LookRotationPitchYaw(endPosition - currentPosition);

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
                        MovementRotation = movementRotation;
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

            ElapsedTimeSec += elapsedDeltaTime;
        }
    }

    [CustomEditor(typeof(TrailMoverBuilder_TargetPos))]
    public class TrailMoverBuilder_TargetPosEditor : AbstractEditor<TrailMoverBuilder_TargetPos>
    {
        private Vector3 StartPosition;
        private Vector3 StartRotation = new Vector3(-45f, 0f, 0f);
        private Vector3 LocalEndPosition = new Vector3(1f, 2f, 1f);
        private float TimeRequiredSec = 0.5f;

        private long StartTime;
        private long LastUpdateTime;
        
        protected override bool OnInitialize(TrailMoverBuilder_TargetPos instance)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {
                SetObserverUpdateCache();

                string electricTrailType = AbilityFXComponentType.ElectricTrail.ToString();
                ElectricTrail electricTrailPrefab = (ElectricTrail) system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == electricTrailType);

                if (electricTrailPrefab != null)
                {
                    ElectricTrail electricTrail = GameObject.Instantiate(electricTrailPrefab, instance.transform);
                    Vector3 position = instance.transform.position;
                    Vector3 endPosition = position + LocalEndPosition;

                    instance.Initialize(ObserverUpdateCache, electricTrail, position, endPosition,
                        StartRotation,
                        TimeRequiredSec);
                    StartPosition = position;
                    TryAddParticleSystem(instance.gameObject);
                    StartTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                    return true;
                }
            }
            return false;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        protected override void ManualUpdate()
        {
            Instance.ManualUpdate();

            LastUpdateTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
        }

        protected override void EditorDestroy()
        {
            StartTime = default;
            GameObject.DestroyImmediate(Instance.ElectricTrail.gameObject);
            Instance.ElectricTrail = null;
            if (LastUpdateTime > StartTime)
            {
                Instance.transform.position = StartPosition;
            }
        }
    }
}
