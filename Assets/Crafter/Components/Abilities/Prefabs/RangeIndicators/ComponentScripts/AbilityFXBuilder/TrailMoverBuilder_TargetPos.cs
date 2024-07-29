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
        private Vector3 CurrentEndPosition;
        [HideInInspector]
        private float TimeRequiredSec;


        [HideInInspector]
        private bool EndPositionUpdateRequested;
        [HideInInspector]
        private Vector3 Rotation;
        [HideInInspector]
        private RotatingCoordinateVector3Angles RotationAngles;
        [HideInInspector]
        private Vector3 LocalPosition;
        [HideInInspector]
        private float ElapsedTimeSec;

        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache,
            ElectricTrail electricTrail,
            Vector3 startPosition, Vector3 endPosition, float timeRequiredSec)
        {
            base.Initialize(observerUpdateCache);

            electricTrail.transform.localPosition = Vector3.zero;
            ElectricTrail = electricTrail;

            StartPosition = startPosition;
            EndPosition = endPosition;
            CurrentEndPosition = endPosition;
            TimeRequiredSec = timeRequiredSec;

            Vector3 rotation = Vector3Util.LookRotationPitchYaw(endPosition - startPosition);
            RotationAngles = new RotatingCoordinateVector3Angles(rotation);

            Rotation = rotation;
            LocalPosition = new Vector3(0f, 0f, 0f);
            ElapsedTimeSec = 0f;
        }

        public void ManualUpdate()
        {
            float elapsedTimeSec = ElapsedTimeSec;
            if (elapsedTimeSec < TimeRequiredSec)
            {
                Vector3 localPosition = LocalPosition;
                Vector3 endPosition = EndPosition;
                RotatingCoordinateVector3Angles rotationAngles = RotationAngles;

                if (EndPositionUpdateRequested)
                {
                    Vector3 rotation = Rotation;
                    float endPositionsDistance = (endPosition - CurrentEndPosition).magnitude;
                    if (endPositionsDistance > 0.03f)
                    {
                        rotation = Vector3Util.LookRotationPitchYaw(endPosition - StartPosition);
                        RotationAngles.SetRotationVector(rotation);
                        Rotation = rotation;
                    }
                    CurrentEndPosition = endPosition;
                    EndPositionUpdateRequested = false;
                }

                float elapsedDeltaTime = ObserverUpdateCache.UpdateTickTimeFixedUpdateDeltaTimeSec;

            }
        }
    }

    [CustomEditor(typeof(TrailMoverBuilder_TargetPos))]
    public class TrailMoverBuilder_TargetPosEditor : AbstractEditor<TrailMoverBuilder_TargetPos>
    {
        private Vector3 StartPosition;
        private Vector3 LocalEndPosition = new Vector3(1f, 2f, 1f);
        private float TimeRequiredSec = 3f;

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

                    instance.Initialize(ObserverUpdateCache, electricTrail, position, endPosition, TimeRequiredSec);
                    TryAddParticleSystem(instance.gameObject);
                    StartPosition = position;
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
