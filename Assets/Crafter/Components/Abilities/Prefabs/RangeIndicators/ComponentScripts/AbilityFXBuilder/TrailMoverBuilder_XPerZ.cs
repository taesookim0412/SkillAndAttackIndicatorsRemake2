using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class TrailMoverBuilder_XPerZ : AbstractAbilityFXBuilder
    {
        private static readonly float TrailRendererYOffset = 0.7f;

        [NonSerialized]
        public ElectricTrail ElectricTrail;

        [HideInInspector]
        public float[] LocalXPositionsPerZUnit;
        [HideInInspector]
        public int LineLength;
        [HideInInspector]
        public (Vector3 worldPosition, Vector3 distanceFromPrev)[] WorldPositionsPerZUnit;
        [HideInInspector]
        private float[] TimeRequiredIncrementalVelocityMult;
        [HideInInspector]
        private Vector3 LocalPosition;
        [HideInInspector]
        private long TimeElapsedForPositionIndex;
        [HideInInspector]
        public int PositionIndex;
        
        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache,
            ElectricTrail electricTrail,
            int lineLength,
            long[] timeRequiredForZDistances,
            SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX, float startPositionZ, float yRotation)
        {
            InitializeManualAwake();

            electricTrail.transform.localPosition = Vector3.zero;
            ElectricTrail = electricTrail;
            LineLength = lineLength;

            if (LocalXPositionsPerZUnit == null || LocalXPositionsPerZUnit.Length != lineLength)
            {
                LocalXPositionsPerZUnit = InitializeLocalXPositionsPerZUnit(lineLength);
            }

            WorldPositionsPerZUnit = InitializeWorldPositionsPerZUnit(skillAndAttackIndicatorSystem,
                startPositionX, startPositionZ, yRotation);

            float[] timeRequiredIncrementalVelocityMult = new float[timeRequiredForZDistances.Length];
            long prevAccumTimeRequiredForZDistance = timeRequiredForZDistances[0];

            for (int i = 1; i < timeRequiredForZDistances.Length; i++)
            {
                long timeRequiredAccum = timeRequiredForZDistances[i];
                long timeRequiredDifference = timeRequiredAccum - prevAccumTimeRequiredForZDistance;
                if (timeRequiredDifference > 0L)
                {
                    timeRequiredIncrementalVelocityMult[i] = 1000f / timeRequiredDifference;
                }
                else
                {
                    timeRequiredIncrementalVelocityMult[i] = 0f;
                }

                prevAccumTimeRequiredForZDistance = timeRequiredAccum;
            }

            TimeRequiredIncrementalVelocityMult = timeRequiredIncrementalVelocityMult;

            LocalPosition = Vector3.zero;
            PositionIndex = 0;
            TimeElapsedForPositionIndex = 0L;
        }
        protected (Vector3 worldPosition, Vector3 distanceFromPrev)[] InitializeWorldPositionsPerZUnit(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX,
            float startPositionZ,
            float yRotation)
        {
            float[] localXPositionsPerZUnit = LocalXPositionsPerZUnit;

            float cosYAngle = (float)Math.Cos(yRotation * Mathf.Deg2Rad);
            float sinYAngle = (float)Math.Sin(yRotation * Mathf.Deg2Rad);

            (Vector3 worldPosition, Vector3 distanceFromPrev)[] worldPositionsTuple = new (Vector3 worldPosition, Vector3 distanceFromPrev)[localXPositionsPerZUnit.Length];

            Vector3 prevWorldPosition = new Vector3(startPositionX, skillAndAttackIndicatorSystem.GetTerrainHeight(startPositionX, startPositionZ), startPositionZ);

            worldPositionsTuple[0].worldPosition = prevWorldPosition;
            worldPositionsTuple[0].distanceFromPrev = Vector3.zero;

            for (int i = 1; i < localXPositionsPerZUnit.Length; i++)
            {
                float localPositionX = localXPositionsPerZUnit[i];

                float rotatedLocalPositionX = i * sinYAngle + localPositionX * cosYAngle;
                float rotatedLocalPositionZ = i * cosYAngle - localPositionX * sinYAngle;

                float worldPositionX = startPositionX + rotatedLocalPositionX;
                float worldPositionZ = startPositionZ + rotatedLocalPositionZ;

                Vector3 worldPosition = new Vector3(worldPositionX, skillAndAttackIndicatorSystem.GetTerrainHeight(worldPositionX, worldPositionZ) + TrailRendererYOffset, worldPositionZ);

                worldPositionsTuple[i] = (worldPosition, distanceFromPrev: worldPosition - prevWorldPosition);

                prevWorldPosition = worldPosition;
            }

            return worldPositionsTuple;
        }

        private float[] InitializeLocalXPositionsPerZUnit(int lineLength)
        {
            float[] xPositions = new float[lineLength];

            int numIterations = (int) Math.Floor(lineLength / 3f);

            for (int i = 0; i < lineLength; i++)
            {
                int xPos = i % 3;

                if (xPos > 1)
                {
                    xPos = -1;
                }

                xPositions[i] = xPos;    
            }

            return xPositions;
        }

        public void ManualUpdate(float fillProgress)
        {
            float zUnits = fillProgress * LineLength;
            int zUnitsIndex = (int)zUnits;
            if (zUnitsIndex > 0)
            {
                if (zUnitsIndex < LineLength)
                {
                    int positionIndex = PositionIndex;
                    if (positionIndex < zUnitsIndex)
                    {
                        //Debug.Log($"{WorldPositionsPerZUnit[positionIndex]}, {transform.position}, {(WorldPositionsPerZUnit[positionIndex].worldPosition - transform.position).magnitude}");
                        transform.position = WorldPositionsPerZUnit[positionIndex].worldPosition;

                        PositionIndex = zUnitsIndex;
                        positionIndex = zUnitsIndex;
                    }
                    //TODO: Interp between PositionIndex and next zUnitsIndex with timeRequiredForDistances and Time.fixedDeltaTime.
                    float dt = Time.fixedDeltaTime * TimeRequiredIncrementalVelocityMult[positionIndex];
                    //Vector3 distanceFromPrevvdt = (WorldPositionsPerZUnit[positionIndex] - transform.position) * dt;

                    float newLocalX = LocalPosition.x + (LocalXPositionsPerZUnit[positionIndex] - LocalXPositionsPerZUnit[positionIndex - 1]) * dt;
                    float newLocalZ = LocalPosition.z + dt;
                    if (newLocalZ > positionIndex)
                    {
                        newLocalX = LocalXPositionsPerZUnit[positionIndex];
                        newLocalZ = positionIndex;

                        transform.position = WorldPositionsPerZUnit[positionIndex].worldPosition;
                    }
                    else
                    {
                        Vector3 distanceFromPrevvdt = (WorldPositionsPerZUnit[positionIndex].distanceFromPrev) * dt;
                        transform.position = transform.position + distanceFromPrevvdt;
                    }
                    
                    LocalPosition.x = newLocalX;
                    LocalPosition.z = newLocalZ;
                    //Debug.Log($"{WorldPositionsPerZUnit[positionIndex].distanceFromPrev}, {dt}, {TimeRequiredVelocityMult[positionIndex]}");
                }
                else
                {
                    PositionIndex = LineLength;
                }
            }
        }
    }

    [CustomEditor(typeof(TrailMoverBuilder_XPerZ))]
    public class TrailMoverBuilder_XPerZEditor : AbstractEditor<TrailMoverBuilder_XPerZ>
    {
        private static readonly int LineLengthUnits = 20;
        private static readonly long ChargeDuration = 5000L;
        private static readonly float ChargeDurationFloat = (float)ChargeDuration;

        private long StartTime;
        private long LastUpdateTime;
        protected override bool OnInitialize(TrailMoverBuilder_XPerZ instance)
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
                    long[] timeRequiredForZDistances = EffectsUtil.GenerateTimeRequiredForDistancesPerUnit(LineLengthUnits, ChargeDuration);
                    instance.Initialize(ObserverUpdateCache, electricTrail, LineLengthUnits, timeRequiredForZDistances, system,
                        position.x, position.z, 0f);
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
            Time.fixedDeltaTime = (ObserverUpdateCache.UpdateTickTimeFixedUpdate - LastUpdateTime) / 1000f;
            float chargeDurationPercentage = (ObserverUpdateCache.UpdateTickTimeFixedUpdate - StartTime) / ChargeDurationFloat;
            float fillProgress = EffectsUtil.EaseInOutQuad(chargeDurationPercentage);
            Instance.ManualUpdate(fillProgress);

            LastUpdateTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
        }

        protected override void EditorDestroy()
        {
            StartTime = default;
            GameObject.DestroyImmediate(Instance.ElectricTrail.gameObject);
            Instance.ElectricTrail = null;
            Instance.transform.position = Instance.WorldPositionsPerZUnit[0].worldPosition;
        }
    }
}
