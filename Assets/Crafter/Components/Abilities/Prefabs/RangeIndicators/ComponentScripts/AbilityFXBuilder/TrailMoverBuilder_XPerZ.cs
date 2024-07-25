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
        public (Vector3 worldPosition, Vector3 distanceFromPrev, float localXPosFromPrev)[] WorldPositionsPerZUnit;
        [HideInInspector]
        private float[] TimeRequiredIncrementalVelocityMult;
        [HideInInspector]
        private float[] TimeRequiredIncrementalSec;
        [HideInInspector]
        private Vector3 LocalPosition;
        [HideInInspector]
        private long TimeElapsedForPositionIndex;
        [HideInInspector]
        public int PositionIndex;
        [HideInInspector]
        private float ElapsedPositionIndexDeltaTime;
        [HideInInspector]
        private Vector3 StartPosition;
        [HideInInspector]
        private float CosYAngle;
        [HideInInspector]
        private float SinYAngle;
        
        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache,
            ElectricTrail electricTrail,
            int lineLength,
            long[] timeRequiredForZDistances,
            SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX, float startPositionZ, float cosYAngle,
            float sinYAngle)
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
                startPositionX, startPositionZ, 
                cosYAngle: cosYAngle,
                sinYAngle: sinYAngle);

            float[] timeRequiredIncrementalVelocityMult = new float[timeRequiredForZDistances.Length];
            float[] timeRequiredIncrementalSec = new float[timeRequiredForZDistances.Length];

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
                timeRequiredIncrementalSec[i] = timeRequiredDifference / 1000f;

                prevAccumTimeRequiredForZDistance = timeRequiredAccum;
            }

            TimeRequiredIncrementalVelocityMult = timeRequiredIncrementalVelocityMult;
            TimeRequiredIncrementalSec = timeRequiredIncrementalSec;

            LocalPosition = new Vector3(0f, 0f, 0f);

            TimeElapsedForPositionIndex = 0L; 
            PositionIndex = 0;
            ElapsedPositionIndexDeltaTime = 0f;
            StartPosition = transform.position;
            CosYAngle = cosYAngle;
            SinYAngle = sinYAngle;
        }
        protected (Vector3 worldPosition, Vector3 distanceFromPrev,
                float localXPosFromPrev)[] InitializeWorldPositionsPerZUnit(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX,
            float startPositionZ,
            float cosYAngle,
            float sinYAngle)
        {
            float[] localXPositionsPerZUnit = LocalXPositionsPerZUnit;

            (Vector3 worldPosition, Vector3 distanceFromPrev,
                float localXFromPrev)[] worldPositionsTuple = new (Vector3 worldPosition, Vector3 distanceFromPrev, float localXFromPrev)[localXPositionsPerZUnit.Length];

            Vector3 prevWorldPosition = new Vector3(startPositionX, skillAndAttackIndicatorSystem.GetTerrainHeight(startPositionX, startPositionZ) + TrailRendererYOffset, startPositionZ);
            float prevLocalXPos = localXPositionsPerZUnit[0];

            worldPositionsTuple[0] = (worldPosition: prevWorldPosition,
                distanceFromPrev: Vector3.zero,
                localXFromPrev: 0f);

            for (int i = 1; i < localXPositionsPerZUnit.Length; i++)
            {
                float localPositionX = localXPositionsPerZUnit[i];

                float rotatedLocalPositionX = i * sinYAngle + localPositionX * cosYAngle;
                float rotatedLocalPositionZ = i * cosYAngle - localPositionX * sinYAngle;

                float worldPositionX = startPositionX + rotatedLocalPositionX;
                float worldPositionZ = startPositionZ + rotatedLocalPositionZ;

                Vector3 worldPosition = new Vector3(worldPositionX, skillAndAttackIndicatorSystem.GetTerrainHeight(worldPositionX, worldPositionZ) + TrailRendererYOffset, worldPositionZ);
                float localXFromPrev = localPositionX - prevLocalXPos;

                worldPositionsTuple[i] = (worldPosition, 
                    distanceFromPrev: worldPosition - prevWorldPosition, 
                    localXFromPrev);

                prevWorldPosition = worldPosition;
                prevLocalXPos = localPositionX;
            }

            return worldPositionsTuple;
        }

        private float[] InitializeLocalXPositionsPerZUnit(int lineLength)
        {
            float[] xPositions = new float[lineLength];

            xPositions[0] = 0f;

            for (int i = 1; i < lineLength; i++)
            {
                int xPos = i & 1;

                if (xPos == 0)
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
                        //transform.position = WorldPositionsPerZUnit[positionIndex].worldPosition;

                        PositionIndex = zUnitsIndex;
                        positionIndex = zUnitsIndex;

                        ElapsedPositionIndexDeltaTime = 0f;
                    }

                    if (LocalPosition.z < positionIndex)
                    {
                        float indexTimeRequiredSec = TimeRequiredIncrementalSec[positionIndex];
                        float elapsedPositionIndexDeltaTime = ElapsedPositionIndexDeltaTime + Time.fixedDeltaTime;
                        float positionIndexDeltaTimePercentage;
                        if (indexTimeRequiredSec > SkillAndAttackIndicatorSystem.FLOAT_TOLERANCE)
                        {
                            positionIndexDeltaTimePercentage = Mathf.Clamp01(elapsedPositionIndexDeltaTime / indexTimeRequiredSec);
                        }
                        else
                        {
                            positionIndexDeltaTimePercentage = 1f;
                        }
                        
                        ElapsedPositionIndexDeltaTime = elapsedPositionIndexDeltaTime;

                        float dt = Time.fixedDeltaTime * TimeRequiredIncrementalVelocityMult[positionIndex];
                        
                        Vector3 currentPosition = transform.position;

                        float newLocalX;
                        float newLocalZ = LocalPosition.z + dt;
                        if (newLocalZ < positionIndex)
                        {
                            //float zDecimals = zUnits - positionIndex;
                            float sinYAngle = SinYAngle;
                            float cosYAngle = CosYAngle;
                            //Vector3 originalVelocity = WorldPositionsPerZUnit[positionIndex].distanceFromPrev;

                            float localXFromPrev = WorldPositionsPerZUnit[positionIndex].localXPosFromPrev;

                            //newLocalX = LocalPosition.x + localXFromPrev * (EffectsUtil.EaseInOutQuad(zDecimals) * dt * 2f);
                            newLocalX = LocalXPositionsPerZUnit[positionIndex - 1] + localXFromPrev * EffectsUtil.EaseInOutQuad(positionIndexDeltaTimePercentage);
                            //Debug.Log($"{elapsedPositionIndexDeltaTime}, {newLocalX}");
                            //newLocalX = LocalPosition.x + localXFromPrev * dt;

                            float currXValue = LocalXPositionsPerZUnit[positionIndex];

                            float rotatedLocalPositionX = newLocalZ * sinYAngle + newLocalX * cosYAngle;
                            //float rotatedLocalPositionZ = newLocalZ * cosYAngle - newLocalX * sinYAngle;

                            float worldPositionX = StartPosition.x + rotatedLocalPositionX;
                            //float worldPositionZ = StartPosition.z + rotatedLocalPositionZ;

                            Vector3 distanceFromPrev = WorldPositionsPerZUnit[positionIndex].distanceFromPrev;
                            float newWorldY = currentPosition.y + distanceFromPrev.y * dt;
                            float newWorldZ = currentPosition.z + distanceFromPrev.z * dt;


                            transform.position = new Vector3(worldPositionX, 
                                newWorldY, newWorldZ);
                        }
                        else
                        {
                            newLocalX = LocalXPositionsPerZUnit[positionIndex];
                            newLocalZ = positionIndex;
                            transform.position = WorldPositionsPerZUnit[positionIndex].worldPosition;
                        }

                        LocalPosition.x = newLocalX;
                        LocalPosition.z = newLocalZ;
                    }
                    
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

                    float cosYAngle = (float)Math.Cos(0f * Mathf.Deg2Rad);
                    float sinYAngle = (float)Math.Sin(0f * Mathf.Deg2Rad);

                    instance.Initialize(ObserverUpdateCache, electricTrail, LineLengthUnits, timeRequiredForZDistances, system,
                        position.x, position.z, cosYAngle, sinYAngle);
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
