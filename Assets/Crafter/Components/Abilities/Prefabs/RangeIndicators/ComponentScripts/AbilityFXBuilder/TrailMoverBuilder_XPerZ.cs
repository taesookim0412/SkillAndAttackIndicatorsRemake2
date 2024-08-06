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
        // lower delay mult = velocity mult lower = trail moves slower.
        //private static readonly float TimeRequiredVelocityDelayMult = 1f;

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
            base.Initialize(observerUpdateCache);

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

            //float oneSecMillisMultByTimeRequiredDelay = 1000f * TimeRequiredVelocityDelayMult;
            //float timeRequiredVelocityMultDivByMillis = TimeRequiredVelocityDelayMult / 1000f;
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
                timeRequiredIncrementalSec[i] = timeRequiredDifference * 0.001f;

                prevAccumTimeRequiredForZDistance = timeRequiredAccum;
            }

            TimeRequiredIncrementalVelocityMult = timeRequiredIncrementalVelocityMult;
            TimeRequiredIncrementalSec = timeRequiredIncrementalSec;

            LocalPosition = new Vector3(0f, 0f, 0f);

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
            int lineLength = LineLength;
            float zUnits = fillProgress * lineLength;
            int zUnitsIndex = (int)zUnits;
            if (zUnitsIndex > 0)
            {
                if (zUnitsIndex < lineLength)
                {
                    Vector3 localPosition = LocalPosition;
                    int positionIndex = PositionIndex;
                    float fixedDeltaTime = ObserverUpdateCache.UpdateTickTimeFixedUpdateDeltaTimeSec;

                    float sinYAngle = SinYAngle;
                    float cosYAngle = CosYAngle;

                    //if (localPosition.z >= positionIndex)
                    //{
                    //    //Debug.Log($"{WorldPositionsPerZUnit[positionIndex]}, {transform.position}, {(WorldPositionsPerZUnit[positionIndex].worldPosition - transform.position).magnitude}");
                    //    //TODO1: interp from pos to worldPos with closest dt multiple.
                    //    //transform.position = WorldPositionsPerZUnit[positionIndex].worldPosition;
                    //    float newLocalX = PositionUtil.CalculateClosestMultipleOrClamp(localPosition.x, LocalXPositionsPerZUnit[positionIndex], fixedDeltaTime);
                    //    float newLocalZ = PositionUtil.CalculateClosestMultipleOrClamp(localPosition.z, positionIndex, fixedDeltaTime);

                    //    localPosition.x = newLocalX;
                    //    localPosition.z = newLocalZ;

                    //    float rotatedLocalPositionX = newLocalZ * sinYAngle + newLocalX * cosYAngle;
                    //    float rotatedLocalPositionZ = newLocalZ * cosYAngle - newLocalX * sinYAngle;

                    //    float worldPositionX = StartPosition.x + rotatedLocalPositionX;
                    //    float worldPositionZ = StartPosition.z + rotatedLocalPositionZ;

                    //    transform.position = new Vector3(worldPositionX, WorldPositionsPerZUnit[positionIndex].worldPosition.y, worldPositionZ);

                    //    LocalPosition = localPosition;

                    //    PositionIndex = ++positionIndex;

                    //    ElapsedPositionIndexDeltaTime = 0f;
                    //}
                    //Debug.Log($"{localPosition.z}, {positionIndex}");
                    if (positionIndex < lineLength)
                    {
                        positionIndex = PositionUtil.MoveTrailPosition(positionIndex, fixedDeltaTime, localPosition.x, localPosition.z,
                            out float newLocalPositionX, out float newLocalPositionZ, TimeRequiredIncrementalSec,
                            TimeRequiredIncrementalVelocityMult, WorldPositionsPerZUnit, LocalXPositionsPerZUnit,
                            ElapsedPositionIndexDeltaTime, out float newElapsedPositionIndexDeltaTime, transform.position.y, out float newWorldPositionY);

                        // Since the position only gets set before the dt, instead of after,
                        // the final position has to be set if the conditions are met

                        if (positionIndex == lineLength)
                        {
                            newLocalPositionX = LocalXPositionsPerZUnit[positionIndex - 1];
                            newLocalPositionZ = positionIndex - 1;
                        }
                        float rotatedLocalPositionX = newLocalPositionZ * sinYAngle + newLocalPositionX * cosYAngle;
                        float rotatedLocalPositionZ = newLocalPositionZ * cosYAngle - newLocalPositionX * sinYAngle;

                        float worldPositionX = StartPosition.x + rotatedLocalPositionX;
                        float worldPositionZ = StartPosition.z + rotatedLocalPositionZ;

                        transform.position = new Vector3(worldPositionX,
                            newWorldPositionY, worldPositionZ);

                        localPosition.x = newLocalPositionX;
                        localPosition.z = newLocalPositionZ;
                        LocalPosition = localPosition;
                        //else
                        //{
                        //    newLocalX = LocalXPositionsPerZUnit[positionIndex];
                        //    newLocalZ = positionIndex;
                        //    //TODO1: interp from pos to worldPos with closest dt multiple.
                        //    transform.position = WorldPositionsPerZUnit[positionIndex].worldPosition;
                        //}
                        //localPosition.x = newLocalX;
                        //localPosition.z = newLocalZ;
                        //LocalPosition = localPosition;

                        

                        PositionIndex = positionIndex;
                        ElapsedPositionIndexDeltaTime = newElapsedPositionIndexDeltaTime;
                        //Debug.Log(PositionIndex);
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
