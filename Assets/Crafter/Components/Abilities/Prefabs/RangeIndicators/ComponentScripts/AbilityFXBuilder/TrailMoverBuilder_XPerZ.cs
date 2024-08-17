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
        private static readonly float TrailRendererYOffset = 0.9f;
        // lower delay mult = velocity mult lower = trail moves slower.
        //private static readonly float TimeRequiredVelocityDelayMult = 1f;

        [NonSerialized]
        public ElectricTrail ElectricTrail;

        [HideInInspector]
        public float[] LocalXPositionsPerZUnit;
        [HideInInspector]
        public int ModifiedLineLength;
        [HideInInspector]
        public int ModifiedLineLengthBuffered;
        [HideInInspector]
        private int ZUnitsPerIndex;
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
            int zUnitsPerIndex,
            long[] timeRequiredForZDistances,
            SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX, float startPositionZ, float cosYAngle,
            float sinYAngle)
        {
            base.Initialize(observerUpdateCache);
            electricTrail.transform.position = new Vector3(startPositionX, skillAndAttackIndicatorSystem.GetTerrainHeight(startPositionX, startPositionZ) + TrailRendererYOffset, startPositionZ);
            electricTrail.ClearParticleSystems();

            ElectricTrail = electricTrail;
            int modifiedLineLength = PartialDataTypesUtil.Round((float)lineLength / (float)zUnitsPerIndex);
            int modifiedLineLengthBuffered = modifiedLineLength + 1;
            ModifiedLineLength = modifiedLineLength;
            ModifiedLineLengthBuffered = modifiedLineLengthBuffered;
            ZUnitsPerIndex = zUnitsPerIndex;

            if (LocalXPositionsPerZUnit == null || LocalXPositionsPerZUnit.Length != modifiedLineLengthBuffered)
            {
                LocalXPositionsPerZUnit = InitializeLocalXPositionsPerIndex(modifiedLineLengthBuffered);
            }

            WorldPositionsPerZUnit = InitializeWorldPositionsPerIndex(skillAndAttackIndicatorSystem,
                startPositionX, startPositionZ, 
                zUnitsPerIndex,
                cosYAngle: cosYAngle,
                sinYAngle: sinYAngle);

            float[] timeRequiredIncrementalVelocityMult = new float[timeRequiredForZDistances.Length];
            float[] timeRequiredIncrementalSec = new float[timeRequiredForZDistances.Length];

            long prevAccumTimeRequiredForIndex = timeRequiredForZDistances[0];

            //float oneSecMillisMultByTimeRequiredDelay = 1000f * TimeRequiredVelocityDelayMult;
            //float timeRequiredVelocityMultDivByMillis = TimeRequiredVelocityDelayMult / 1000f;
            for (int i = 1; i < timeRequiredForZDistances.Length; i++)
            {
                long timeRequiredAccum = timeRequiredForZDistances[i];
                long timeRequiredDifference = timeRequiredAccum - prevAccumTimeRequiredForIndex;
                if (timeRequiredDifference > 0L)
                {
                    timeRequiredIncrementalVelocityMult[i] = 1000f / timeRequiredDifference;
                }
                else
                {
                    timeRequiredIncrementalVelocityMult[i] = 0f;
                }
                timeRequiredIncrementalSec[i] = timeRequiredDifference * 0.001f;

                prevAccumTimeRequiredForIndex = timeRequiredAccum;
            }

            TimeRequiredIncrementalVelocityMult = timeRequiredIncrementalVelocityMult;
            TimeRequiredIncrementalSec = timeRequiredIncrementalSec;

            LocalPosition = new Vector3(0f, 0f, 0f);

            PositionIndex = 1;
            ElapsedPositionIndexDeltaTime = 0f;
            StartPosition = transform.position;
            CosYAngle = cosYAngle;
            SinYAngle = sinYAngle;
        }
        protected (Vector3 worldPosition, Vector3 distanceFromPrev,
                float localXPosFromPrev)[] InitializeWorldPositionsPerIndex(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX,
            float startPositionZ,
            int zUnitsPerIndex,
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
                float localPositionZ = i * zUnitsPerIndex;

                float rotatedLocalPositionX = localPositionZ * sinYAngle + localPositionX * cosYAngle;
                float rotatedLocalPositionZ = localPositionZ * cosYAngle - localPositionX * sinYAngle;

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

        private float[] InitializeLocalXPositionsPerIndex(int bufferedModifiedLineLength)
        {
            float[] xPositions = new float[bufferedModifiedLineLength];

            xPositions[0] = 0f;

            for (int i = 1; i < bufferedModifiedLineLength; i++)
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
            int modifiedLineLengthBuffered = ModifiedLineLengthBuffered;
            float zUnits = fillProgress * ModifiedLineLength;
            int zUnitsIndex = (int)zUnits;
            if (zUnitsIndex < modifiedLineLengthBuffered)
            {
                Debug.Log($"{fillProgress}: {zUnitsIndex}, {LocalPosition.z}, {PositionIndex}");

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
                if (positionIndex < modifiedLineLengthBuffered)
                {
                    positionIndex = PositionUtil.MoveTrailPosition(positionIndex, fixedDeltaTime, localPosition.x, localPosition.z,
                        out float newLocalPositionX, out float newLocalPositionZ, TimeRequiredIncrementalSec,
                        TimeRequiredIncrementalVelocityMult, WorldPositionsPerZUnit, LocalXPositionsPerZUnit, ZUnitsPerIndex,
                        ElapsedPositionIndexDeltaTime, out float newElapsedPositionIndexDeltaTime, ElectricTrail.transform.position.y, out float newWorldPositionY);

                    // Since the position only gets set before the dt, instead of after,
                    // the final position has to be set if the conditions are met

                    if (positionIndex == modifiedLineLengthBuffered)
                    {
                        newLocalPositionX = LocalXPositionsPerZUnit[positionIndex - 1];
                        newLocalPositionZ = (positionIndex - 1) * ZUnitsPerIndex;
                    }
                    float rotatedLocalPositionX = newLocalPositionZ * sinYAngle + newLocalPositionX * cosYAngle;
                    float rotatedLocalPositionZ = newLocalPositionZ * cosYAngle - newLocalPositionX * sinYAngle;

                    float worldPositionX = StartPosition.x + rotatedLocalPositionX;
                    float worldPositionZ = StartPosition.z + rotatedLocalPositionZ;

                    ElectricTrail.transform.position = new Vector3(worldPositionX,
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
                    //Debug.Log(newElapsedPositionIndexDeltaTime);
                }

                //Debug.Log($"{WorldPositionsPerZUnit[positionIndex].distanceFromPrev}, {dt}, {TimeRequiredVelocityMult[positionIndex]}");
            }
            else
            {
                PositionIndex = ModifiedLineLength;
            }
        }
        public override void CleanUpInstance()
        {
            ElectricTrail = null;
            WorldPositionsPerZUnit = null;
        }
    }

    [CustomEditor(typeof(TrailMoverBuilder_XPerZ))]
    public class TrailMoverBuilder_XPerZEditor : AbstractEditor<TrailMoverBuilder_XPerZ>
    {
        public int ZUnitsPerIndex = 5;
        private static readonly int LineLengthUnits = 20;
        private static readonly long ChargeDuration = 800L;
        private static readonly float ChargeDurationFloat = (float)ChargeDuration;

        private long StartTime;
        private long LastUpdateTime;
        protected override bool OnInitialize(TrailMoverBuilder_XPerZ instance, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {
                string electricTrailType = AbilityFXComponentType.ElectricTrail.ToString();
                ElectricTrail electricTrailPrefab = (ElectricTrail) system.AbilityFXComponentPrefabs.FirstOrDefault(prefab => prefab.name == electricTrailType);

                if (electricTrailPrefab != null)
                {
                    ElectricTrail electricTrail = GameObject.Instantiate(electricTrailPrefab, instance.transform);
                    electricTrail.transform.localPosition = Vector3.zero;
                    Vector3 position = instance.transform.position;
                    long[] timeRequiredForZDistances = EffectsUtil.GenerateTimeRequiredForDistancesPerModifiedUnit(LineLengthUnits, ChargeDuration, ZUnitsPerIndex);

                    float cosYAngle = (float)Math.Cos(0f * Mathf.Deg2Rad);
                    float sinYAngle = (float)Math.Sin(0f * Mathf.Deg2Rad);

                    if (observerUpdateCache == null)
                    {
                        SetObserverUpdateCache();
                        observerUpdateCache = ObserverUpdateCache;
                    }
                    instance.Initialize(observerUpdateCache, electricTrail, LineLengthUnits, ZUnitsPerIndex, timeRequiredForZDistances, system,
                        position.x, position.z, cosYAngle, sinYAngle);
                    TryAddParticleSystem(instance.gameObject);
                    StartTime = observerUpdateCache.UpdateTickTimeFixedUpdate;
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
            Instance.transform.position = Instance.WorldPositionsPerZUnit[0].worldPosition;
            Instance.CleanUpInstance();
        }
    }
}
