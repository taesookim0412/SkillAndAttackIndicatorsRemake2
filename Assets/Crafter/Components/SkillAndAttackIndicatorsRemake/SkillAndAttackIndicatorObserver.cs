using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;
using Assets.Crafter.Components.Models;
using DTT.AreaOfEffectRegions;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using UnityEditor;
using StarterAssets;
using Assets.Crafter.Components.Player.ComponentScripts;

using Random = System.Random;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder;
using Assets.Crafter.Components.Systems.Observers;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class SkillAndAttackIndicatorObserverProps
    {
        public SkillAndAttackIndicatorSystem SkillAndAttackIndicatorSystem;

        public ObserverUpdateCache ObserverUpdateCache;
        public SkillAndAttackIndicatorObserverProps(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem = skillAndAttackIndicatorSystem;
            ObserverUpdateCache = observerUpdateCache;
        }
    }
    public class SkillAndAttackIndicatorObserver
    {
        public static readonly string[] AbilityProjectorTypeNames = Enum.GetNames(typeof(AbilityProjectorType));
        public static readonly int AbilityProjectorTypeNamesLength = AbilityProjectorTypeNames.Length;
        public static readonly string[] AbilityProjectorMaterialTypeNames = Enum.GetNames(typeof(AbilityProjectorMaterialType));
        public static readonly int AbilityProjectorMaterialTypeNamesLength = AbilityProjectorMaterialTypeNames.Length;
        public static readonly string[] AbilityFXTypeNames = Enum.GetNames(typeof(AbilityIndicatorFXType));
        public static readonly int AbilityFXTypeNamesLength = AbilityFXTypeNames.Length;
        public static readonly string[] AbilityFXComponentTypeNames = Enum.GetNames(typeof(AbilityFXComponentType));
        public static readonly int AbilityFXComponentTypeNamesLength = AbilityFXComponentTypeNames.Length;

        private static readonly Random Random = new Random();
        private static readonly float RadiusHalfDivMult = 1 / 2f;
        // The orthographic length is based on a radius so it is half the desired length.
        // Then, it must be multiplied by half again because it is a "half-length" in the documentation.
        // private static readonly float OrthographicRadiusHalfDivMult = 1 / 4f;

        // ... hardcoded
        private static readonly int LineLengthUnits = 20;
        // Also used for adding ArcPathFromCloneOffset
        private static readonly int PortalSpotOffsetUnits = 2;
        private static readonly int UnitsPerPortalSpot = 5;
        private static readonly float TrailRendererYOffset = 0.7f;


        private SkillAndAttackIndicatorObserverProps Props;

        public ObserverStatus ObserverStatus = ObserverStatus.Active;

        public readonly AbilityProjectorType AbilityProjectorType;
        public readonly AbilityProjectorMaterialType AbilityProjectorMaterialType;
        public readonly AbilityIndicatorCastType AbilityIndicatorCastType;
        public readonly AbilityIndicatorFXType[] AbilityIndicatorFXTypes;
        public readonly Guid PlayerGuid;

        private bool ProjectorSet = false;
        private PoolBagDco<MonoBehaviour> ProjectorInstancePool;
        private PoolBagDco<AbstractAbilityFX>[][] AbilityFXInstancePools;
        private MonoBehaviour ProjectorMonoBehaviour;
        private PlayerComponent PlayerComponent;
        private PoolBagDco<PlayerComponent> PlayerCloneInstancePool;

        private SRPArcRegionProjector ArcRegionProjectorRef;
        private SRPCircleRegionProjector CircleRegionProjectorRef;
        private SRPLineRegionProjector LineRegionProjectorRef;
        private SRPScatterLineRegionProjector ScatterLineRegionProjectorRef;

        private (DashParticles[] dashParticles, ElectricTrailRenderer electricTrailRenderer,
            bool[] portalSpotsPassed,
            int numElectricTrailRendererPositions,
            int lastArcPathsIndex) DashParticlesItems;

        private long ChargeDuration;
        private float ChargeDurationSecondsFloat;

        private long[] TimeRequiredForDistancesPerUnit;

        private Vector3 PreviousTerrainProjectorPosition;
        private float PreviousRotationY;
        private float PreviousChargeDurationFloatPercentage;

        private long LastTickTime;
        private long ElapsedTime;
        private float ElapsedTimeSecondsFloat;

        public SkillAndAttackIndicatorObserver(AbilityProjectorType abilityProjectorType,
            AbilityProjectorMaterialType abilityProjectorMaterialType, AbilityIndicatorCastType abilityIndicatorCastType,
            AbilityIndicatorFXType[] abilityIndicatorFXTypes,
            SkillAndAttackIndicatorObserverProps skillAndAttackIndicatorObserverProps
            )
        {
            AbilityProjectorType = abilityProjectorType;
            AbilityProjectorMaterialType = abilityProjectorMaterialType;
            AbilityIndicatorCastType = abilityIndicatorCastType;
            AbilityIndicatorFXTypes = abilityIndicatorFXTypes;
            PlayerGuid = skillAndAttackIndicatorObserverProps.SkillAndAttackIndicatorSystem.PlayerGuid;

            Props = skillAndAttackIndicatorObserverProps;
        }

        public void OnUpdate()
        {
            if (!ProjectorSet)
            {
                if (Props.SkillAndAttackIndicatorSystem.ProjectorInstancePools.TryGetValue(AbilityProjectorType, out var abilityMaterialTypesDict) &&
                    abilityMaterialTypesDict.TryGetValue(AbilityProjectorMaterialType, out ProjectorInstancePool) &&
                    (AbilityIndicatorFXTypes == null ||
                    (Props.SkillAndAttackIndicatorSystem.AbilityIndicatorFXInstancePools.TryGetValuesAll(AbilityIndicatorFXTypes, out AbilityFXInstancePools) &&
                    Props.SkillAndAttackIndicatorSystem.PlayerCloneInstancePools.TryGetValue(PlayerGuid, out PlayerCloneInstancePool))))
                {
                    // 3 texture option indices.
                    ProjectorMonoBehaviour = ProjectorInstancePool.InstantiatePooled(null);

                    // Create the projector.

                    // hard coded lengths that need to be used in fx too.

                    Vector3 terrainProjectorPosition = GetTerrainProjectorPosition();
                    float playerRotation = GetThirdPersonControllerRotation();

                    switch (AbilityProjectorType)
                    {
                        case AbilityProjectorType.Arc:

                            SRPArcRegionProjector arcRegionProjector = ProjectorMonoBehaviour.GetComponent<SRPArcRegionProjector>();
                            arcRegionProjector.Radius = 70;
                            //arcRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);

                            arcRegionProjector.GenerateProjector();

                            ArcRegionProjectorRef = arcRegionProjector;
                            break;
                        case AbilityProjectorType.Circle:
                            SRPCircleRegionProjector circleRegionProjector = ProjectorMonoBehaviour.GetComponent<SRPCircleRegionProjector>();
                            circleRegionProjector.Radius = 70;

                            //circleRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);
                            circleRegionProjector.GenerateProjector();

                            CircleRegionProjectorRef = circleRegionProjector;
                            break;
                        case AbilityProjectorType.Line:
                            SRPLineRegionProjector lineRegionProjector = ProjectorMonoBehaviour.GetComponent<SRPLineRegionProjector>();

                            // multiply it by the orthographicRadiusHalfDivMultiplier
                            //float orthographicLength = lineLengthUnits * OrthographicRadiusHalfDivMult;
                            lineRegionProjector.Length = LineLengthUnits * RadiusHalfDivMult;

                            //lineRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);
                            lineRegionProjector.GenerateProjector();
                            lineRegionProjector.SetTerrainLayer(Props.SkillAndAttackIndicatorSystem.TerrainRenderingLayer);
                            lineRegionProjector.Angle = playerRotation;
                            lineRegionProjector.Depth = 100f;
                            lineRegionProjector.Width = 2f;

                            // this should be done after, not within generateprojector...
                            lineRegionProjector.UpdateProjectors();

                            LineRegionProjectorRef = lineRegionProjector;
                            break;
                        case AbilityProjectorType.ScatterLine:
                            SRPScatterLineRegionProjector scatterLineRegionProjector = ProjectorMonoBehaviour.GetComponent<SRPScatterLineRegionProjector>();
                            scatterLineRegionProjector.Length = 70;
                            scatterLineRegionProjector.Add(3);

                            scatterLineRegionProjector.GenerateProjector();

                            ScatterLineRegionProjectorRef = scatterLineRegionProjector;
                            // SetIgnoreLayers not supported with the current scatterlineregionprojector...
                            break;
                        default:
                            ObserverStatus = ObserverStatus.Remove;
                            return;
                    }

                    ProjectorMonoBehaviour.transform.position = terrainProjectorPosition;
                    PreviousTerrainProjectorPosition = terrainProjectorPosition;
                    PreviousRotationY = playerRotation;

                    switch (AbilityProjectorMaterialType)
                    {
                        case AbilityProjectorMaterialType.First:
                            ChargeDuration = 3000L;
                            ChargeDurationSecondsFloat = 3000 * 0.001f;
                            break;
                        //case AbilityProjectorMaterialType.Second:
                        //    ChargeDuration = 5000L;
                        //    ChargeDurationSecondsFloat = 5000 * 0.001f;
                        //    break;
                        //case AbilityProjectorMaterialType.Third:
                        //    ChargeDuration = 7000L;
                        //    ChargeDurationSecondsFloat = 7000 * 0.001f;
                        //    break;
                        default:
                            ObserverStatus = ObserverStatus.Remove;
                            return;
                    }

                    TimeRequiredForDistancesPerUnit = GenerateTimeRequiredForDistancesPerUnit();

                    if (AbilityIndicatorFXTypes != null)
                    {
                        for (int i = 0; i < AbilityIndicatorFXTypes.Length; i++)
                        {
                            AbilityIndicatorFXType abilityFXType = AbilityIndicatorFXTypes[i];
                            switch (abilityFXType)
                            {
                                case AbilityIndicatorFXType.DashParticles:
                                    DashParticlesItems = CreateDashParticlesItems(LineLengthUnits,
                                        terrainProjectorPosition.x, terrainProjectorPosition.z, GetThirdPersonControllerRotation(),
                                        i);
                                    break;
                            }
                        }
                    }



                    ProjectorSet = true;
                    LastTickTime = Props.ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                }
                else
                {
                    ObserverStatus = ObserverStatus.Remove;
                    return;
                }
            }
            else
            {
                long elapsedTickTime = Props.ObserverUpdateCache.UpdateTickTimeFixedUpdate - LastTickTime;
                LastTickTime = Props.ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                ElapsedTime += elapsedTickTime;
                ElapsedTimeSecondsFloat += elapsedTickTime * 0.001f;

                float newFillProgress = 0f;
                switch (AbilityProjectorType)
                {
                    case AbilityProjectorType.Arc:
                        break;
                    case AbilityProjectorType.Circle:
                        break;

                    case AbilityProjectorType.Line:
                        if (PreviousChargeDurationFloatPercentage < 1f)
                        {
                            float chargeDurationPercentage = ElapsedTimeSecondsFloat / ChargeDurationSecondsFloat;

                            if (chargeDurationPercentage > PreviousChargeDurationFloatPercentage)
                            {
                                newFillProgress = EaseInOutQuad(chargeDurationPercentage);
                                LineRegionProjectorRef.FillProgress = newFillProgress;
                                LineRegionProjectorRef.UpdateProjectors();
                                PreviousChargeDurationFloatPercentage = chargeDurationPercentage;
                            }
                            else
                            {
                                newFillProgress = EaseInOutQuad(PreviousChargeDurationFloatPercentage);
                            }
                        }
                        break;

                    case AbilityProjectorType.ScatterLine:

                        break;

                }

                Vector3 terrainProjectorPosition = GetTerrainProjectorPosition();
                float playerRotation = GetThirdPersonControllerRotation();
                
                ProjectorMonoBehaviour.transform.position = terrainProjectorPosition;

                Vector3 previousProjectorRotation = ProjectorMonoBehaviour.transform.localEulerAngles;
                ProjectorMonoBehaviour.transform.localEulerAngles = new Vector3(previousProjectorRotation.x, playerRotation, previousProjectorRotation.z);

                if (AbilityIndicatorFXTypes != null)
                {
                    float rotationDifference = playerRotation - PreviousRotationY;
                    if (rotationDifference < -10f || rotationDifference > 10f ||
                        (PreviousTerrainProjectorPosition - terrainProjectorPosition).magnitude > 0.03f)
                    {
                        foreach (AbilityIndicatorFXType abilityFXType in AbilityIndicatorFXTypes)
                        {
                            switch (abilityFXType)
                            {
                                case AbilityIndicatorFXType.DashParticles:
                                    UpdateDashParticlesItemsPositions(LineLengthUnits, terrainProjectorPosition.x, terrainProjectorPosition.z, playerRotation,
                                        fillProgress: newFillProgress);
                                    break;
                            }
                        }
                        
                        PreviousTerrainProjectorPosition = terrainProjectorPosition;
                        PreviousRotationY = playerRotation;
                    }

                    foreach (AbilityIndicatorFXType abilityFXType in AbilityIndicatorFXTypes)
                    {
                        switch (abilityFXType)
                        {
                            case AbilityIndicatorFXType.DashParticles:
                                UpdateDashParticlesItems(LineLengthUnits, newFillProgress);
                                break;
                        }
                    }
                    // every update
                }
            }

            if (ElapsedTime > ChargeDuration)
            {
                Props.SkillAndAttackIndicatorSystem.AddDashAbilityTriggerObserver(DashParticlesItems.dashParticles[DashParticlesItems.dashParticles.Length - 1].transform.position);

                ProjectorInstancePool.ReturnPooled(ProjectorMonoBehaviour);
                if (AbilityIndicatorFXTypes != null)
                {
                    for (int i = 0; i < AbilityIndicatorFXTypes.Length; i++)
                    {
                        PoolBagDco<AbstractAbilityFX>[] abilityFXInstancePool = AbilityFXInstancePools[i];
                        switch (AbilityIndicatorFXTypes[i])
                        {
                            case AbilityIndicatorFXType.DashParticles:
                                PoolBagDco<AbstractAbilityFX> dashParticlesPool = abilityFXInstancePool[(int) DashParticlesFXTypeInstancePools.DashParticles];
                                PoolBagDco<AbstractAbilityFX> electricTrailRendererPool = abilityFXInstancePool[(int)DashParticlesFXTypeInstancePools.ElectricTrailRenderer];

                                foreach (DashParticles dashParticles in DashParticlesItems.dashParticles)
                                {
                                    dashParticlesPool.ReturnPooled(dashParticles);
                                }

                                DashParticlesItems.electricTrailRenderer.CleanUpInstance();
                                electricTrailRendererPool.ReturnPooled(DashParticlesItems.electricTrailRenderer);
                                break;
                        }
                    }
                }

                ObserverStatus = ObserverStatus.Remove;
            }
        }


        private long[] GenerateTimeRequiredForDistancesPerUnit()
        {
            // iterate the distances
            // find the fillPercentage
            // invert the fillPercentage into time
            int lineLengthUnits = LineLengthUnits;
            float lineLengthUnitsFloat = (float) lineLengthUnits;

            float chargeDurationFloat = (float) ChargeDuration;

            int startDistUnitsIndex = 0;
            long prevChargeDurationIterationTimeRequired = 0L;

            long[] timeRequiredForDistancesPerUnit = new long[lineLengthUnits];
            for (int i = 0; i < lineLengthUnits; i++)
            {
                // although this is fillProgress in the other algorithms, here it is the 
                // i = 10, % = 0.33
                float lineLengthPercentage = i / lineLengthUnitsFloat;
                // i = 10, fp% = ~0.1089
                float fillProgress = EaseInOutQuad(lineLengthPercentage);
                // i = 10, zDist = 0.11 * 30 = 3.3
                int zDistanceUnitsIndex = (int) (fillProgress * lineLengthUnits);
                long timeValue = (long) (lineLengthPercentage * chargeDurationFloat);
                // i = 10, fill prevIdx to 3.3 (or zDistanceUnits) with timePercentage.

                int interpLen = zDistanceUnitsIndex - startDistUnitsIndex;
                float interpLenFloat = (float) interpLen;
                long timeValueDifference = timeValue - prevChargeDurationIterationTimeRequired;
                for (int j = 0; j < interpLen; j++)
                {
                    long interpTimeValue = (long)(prevChargeDurationIterationTimeRequired + timeValueDifference * ((j + 1) / interpLenFloat));
                    int interpIndex = startDistUnitsIndex + j;
                    if (interpIndex < lineLengthUnits)
                    {
                        timeRequiredForDistancesPerUnit[interpIndex] = interpTimeValue;
                    }
                    else
                    {
                        // fp error...? let it fill old values...
                        Debug.LogError("FP error when interp-ing distance times...");
                        break;
                    }
                    if (j == interpLen - 1)
                    {
                        prevChargeDurationIterationTimeRequired = interpTimeValue;
                        startDistUnitsIndex += interpLen; 
                    }
                }

            }
            // fill the remaining values...
            if (startDistUnitsIndex < lineLengthUnits)
            {
                long lastValue = startDistUnitsIndex > 0 ? timeRequiredForDistancesPerUnit[startDistUnitsIndex - 1] : 0L;
                for (int i = startDistUnitsIndex; i < lineLengthUnits; i++)
                {
                    timeRequiredForDistancesPerUnit[i] = lastValue;
                }
            }

            return timeRequiredForDistancesPerUnit;
        }
        //private long[] GenerateTimeRequiredForDistancesPerUnit()
        //{
        //    // iterate per 0.1sec
        //    // get the time / total time
        //    // fillProgress = convert into eased time
        //    // since lineLengthPercentage = unitsIndex / totalUnits,
        //    // index = minimize the difference between fillProgress and lineLengthPercentage
        //    // fill prevIndex... (index)

        //    int distUnitsIndex = 0;
        //    int lineLengthUnits = LineLengthUnits;
        //    float lineLengthUnitsFloat = (float) lineLengthUnits;
        //    long prevChargeDurationIterationTimeRequired = 0L;

        //    long[] timeRequiredForDistancesPerUnit = new long[lineLengthUnits];
        //    // iterate based on 0.1 sec interval
        //    int numChargeDurationIterations = (int)Math.Ceiling(ChargeDurationSecondsFloat * 10);
        //    for (int i = 0; i < numChargeDurationIterations; i++)
        //    {
        //        // divide by 10 then mult by 1000 to convert to millis.
        //        long iterationTimeValue = i * 100;

        //        float chargeDurationPercentage = i * 0.1f / ChargeDurationSecondsFloat;
        //        float fillProgress = EaseInOutQuad(chargeDurationPercentage);

        //        // set to max
        //        bool minimumDifferenceFound = false;

        //        int startDistUnitsIndex = distUnitsIndex;
        //        float prevFillPercentageDifference = 1f;
        //        // since the nested loop occurs so frequently, it increments too much... So, this algorithm is abandoned and instead, an Ease Invert is used in another function.
        //        while (distUnitsIndex < lineLengthUnits)
        //        {
        //            float lineLengthPercentage = distUnitsIndex / lineLengthUnitsFloat;
        //            float lineLengthFillProgressDifferenceAbsolute = Mathf.Abs(lineLengthPercentage - fillProgress);

        //            if (lineLengthFillProgressDifferenceAbsolute > prevFillPercentageDifference)
        //            {
        //                minimumDifferenceFound = true;
        //            }
        //            else
        //            {
        //                prevFillPercentageDifference = lineLengthFillProgressDifferenceAbsolute;
        //            }

        //            //// Instead of setting here, the values are interpolated below.
        //            //timeRequiredForDistancesPerUnit[distUnitsIndex] = iterationTimeValue;

        //            distUnitsIndex++;
        //            // break after iteration, thus indices are exclusive of range of distUnitsIndex
        //            if (minimumDifferenceFound)
        //            {
        //                break;
        //            }
        //        }

        //        long iterationTimeDifference = iterationTimeValue - prevChargeDurationIterationTimeRequired;

        //        int interpLen = distUnitsIndex - startDistUnitsIndex;
        //        float interLenFloat = (float) interpLen;
        //        for (int j = 0; j < interpLen; j++)
        //        {
        //            long timeRequired = (long) (prevChargeDurationIterationTimeRequired + iterationTimeDifference * ((j + 1) / interLenFloat));

        //            timeRequiredForDistancesPerUnit[startDistUnitsIndex + j] = timeRequired;

        //            if (j == interpLen - 1)
        //            {
        //                prevChargeDurationIterationTimeRequired = timeRequired;
        //            }
        //        }
        //    }

        //    // fill the remaining values...
        //    if (distUnitsIndex < lineLengthUnits)
        //    {
        //        long lastValue = distUnitsIndex > 0 ? timeRequiredForDistancesPerUnit[distUnitsIndex - 1] : 0L;
        //        for (int i = distUnitsIndex; i < lineLengthUnits; i++)
        //        {
        //            timeRequiredForDistancesPerUnit[i] = lastValue;
        //        }
        //    }

        //    return timeRequiredForDistancesPerUnit;
        //}

        private float GetThirdPersonControllerRotation()
        {
            if (Props.SkillAndAttackIndicatorSystem.PlayerComponent != null)
            {
                return Props.SkillAndAttackIndicatorSystem.PlayerComponent.transform.localEulerAngles.y;
            }
            return 0f;
        }

        private (DashParticles[] dashParticles,
            ElectricTrailRenderer electricTrailRenderer,
            bool[] portalSpotsPassed,
            int numElectricTrailRendererPositions,
            int lastArcPathsIndex) CreateDashParticlesItems(int lineLengthUnits,
            float startPositionX, float startPositionZ,
            float yRotation, int abilityFXIndex)
        {
            int numPortalSpots = (int)Math.Floor((lineLengthUnits - PortalSpotOffsetUnits) / (float)UnitsPerPortalSpot);

            DashParticles[] dashParticles = new DashParticles[lineLengthUnits];
            bool[] portalSpotsPassed = new bool[numPortalSpots];

            float cosYAngle = (float)Math.Cos(yRotation * Mathf.Deg2Rad);
            float sinYAngle = (float)Math.Sin(yRotation * Mathf.Deg2Rad);

            Vector3 yRotationVector = new Vector3(0f, yRotation, 0f);

            float worldRotatedPositionX = startPositionX;
            float worldRotatedPositionZ = startPositionZ;

            PoolBagDco<AbstractAbilityFX>[] dashParticlesTypeFXPools = AbilityFXInstancePools[abilityFXIndex];

            PoolBagDco<AbstractAbilityFX> dashParticlesInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.DashParticles];

            float previousPositionY = 0f;
            float prevXAnglei0 = 0f;
            float prevXAnglei1 = 0f;
            for (int i = 0; i < lineLengthUnits; i++)
            {
                DashParticles dashParticlesComponent = (DashParticles)dashParticlesInstancePool.InstantiatePooled(null);
                // set inactive when created.
                dashParticlesComponent.gameObject.SetActive(false);
                float positionY = Props.SkillAndAttackIndicatorSystem.GetTerrainHeight(worldRotatedPositionX, worldRotatedPositionZ);

                AnimationCurve yVelocityAnimCurve = CreateTerrainYVelocityAnimationCurve(
                    unitsPerKeyframe: 0.05f,
                    worldStartRotatedPositionX: worldRotatedPositionX,
                    positionY: positionY,
                    worldStartRotatedPositionZ: worldRotatedPositionZ,
                    cosYAngle: cosYAngle,
                    sinYAngle: sinYAngle);

                float xAngle;
                if (i > 1)
                {
                    xAngle = (((float)Math.Atan((positionY - previousPositionY)) * Mathf.Rad2Deg * -1f) + prevXAnglei0 + prevXAnglei1) / 3f;
                }
                else if (i == 1)
                {
                    float nextPositionY = Props.SkillAndAttackIndicatorSystem.GetTerrainHeight(worldRotatedPositionX + sinYAngle, worldRotatedPositionZ + cosYAngle);
                    xAngle = (((float)Math.Atan((nextPositionY - positionY)) * Mathf.Rad2Deg * -1f) + prevXAnglei1) / 2f;
                }
                else
                {
                    float nextPositionY = Props.SkillAndAttackIndicatorSystem.GetTerrainHeight(worldRotatedPositionX + sinYAngle, worldRotatedPositionZ + cosYAngle);
                    xAngle = ((float)Math.Atan((nextPositionY - positionY)) * Mathf.Rad2Deg * -1f);
                }

                dashParticlesComponent.SetYVelocityAnimationCurve(yVelocityAnimCurve);
                dashParticlesComponent.SetXAngle(xAngle);

                dashParticlesComponent.transform.position = new Vector3(worldRotatedPositionX,
                    positionY,
                    worldRotatedPositionZ);
                dashParticlesComponent.transform.localEulerAngles = yRotationVector;

                dashParticles[i] = dashParticlesComponent;

                worldRotatedPositionX += sinYAngle;
                worldRotatedPositionZ += cosYAngle;
                previousPositionY = positionY;
                prevXAnglei0 = prevXAnglei1;
                prevXAnglei1 = xAngle;
            }

            PoolBagDco<AbstractAbilityFX> electricTrailRendererInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.ElectricTrailRenderer];

            ElectricTrailRenderer electricTrailRenderer = (ElectricTrailRenderer) electricTrailRendererInstancePool.InstantiatePooled(dashParticles[0].transform.position);

            return (dashParticles, electricTrailRenderer, portalSpotsPassed, 1, -1);
        }
        private void UpdateDashParticlesItemsPositions(int lineLengthUnits,
            float startPositionX, float startPositionZ,
            float yRotation, float fillProgress)
        {
            float cosYAngle = (float)Math.Cos(yRotation * Mathf.Deg2Rad);
            float sinYAngle = (float)Math.Sin(yRotation * Mathf.Deg2Rad);

            Vector3 yRotationVector = new Vector3(0f, yRotation, 0f);

            float worldRotatedPositionX = startPositionX;
            float worldRotatedPositionZ = startPositionZ;

            float previousPositionY = 0f;
            float prevXAnglei0 = 0f;
            float prevXAnglei1 = 0f;

            DashParticles[] dashParticlesArray = DashParticlesItems.dashParticles;
            for (int i = 0; i < lineLengthUnits; i++)
            {
                float positionY = Props.SkillAndAttackIndicatorSystem.GetTerrainHeight(worldRotatedPositionX, worldRotatedPositionZ);

                AnimationCurve yVelocityAnimCurve = CreateTerrainYVelocityAnimationCurve(
                    unitsPerKeyframe: 0.05f,
                    worldStartRotatedPositionX: worldRotatedPositionX,
                    positionY: positionY,
                    worldStartRotatedPositionZ: worldRotatedPositionZ,
                    cosYAngle: cosYAngle,
                    sinYAngle: sinYAngle);

                float xAngle;
                if (i > 1)
                {
                    xAngle = (((float)Math.Atan((positionY - previousPositionY)) * Mathf.Rad2Deg * -1f) + prevXAnglei0 + prevXAnglei1) / 3f;
                }
                else if (i == 1)
                {
                    float nextPositionY = Props.SkillAndAttackIndicatorSystem.GetTerrainHeight(worldRotatedPositionX + sinYAngle, worldRotatedPositionZ + cosYAngle);
                    xAngle = (((float)Math.Atan((nextPositionY - positionY)) * Mathf.Rad2Deg * -1f) + prevXAnglei1) / 2f;
                }
                else
                {
                    float nextPositionY = Props.SkillAndAttackIndicatorSystem.GetTerrainHeight(worldRotatedPositionX + sinYAngle, worldRotatedPositionZ + cosYAngle);
                    xAngle = ((float)Math.Atan((nextPositionY - positionY)) * Mathf.Rad2Deg * -1f);
                }

                DashParticles dashParticles = dashParticlesArray[i];
                dashParticles.SetYVelocityAnimationCurve(yVelocityAnimCurve);
                dashParticles.SetXAngle(xAngle);
                dashParticles.transform.position = new Vector3(worldRotatedPositionX,
                    positionY,
                    worldRotatedPositionZ);
                dashParticles.transform.localEulerAngles = yRotationVector;

                worldRotatedPositionX += sinYAngle;
                worldRotatedPositionZ += cosYAngle;
                previousPositionY = positionY;
                prevXAnglei0 = prevXAnglei1;
                prevXAnglei1 = xAngle;
            }



            Vector3[] worldElectricTrailRendererPositions = new Vector3[DashParticlesItems.numElectricTrailRendererPositions];
            worldElectricTrailRendererPositions[0] = dashParticlesArray[0].transform.position;
            
            for (int i = 1; i < worldElectricTrailRendererPositions.Length; i++)
            {
                int portalSpotIndex = Math.Clamp(PortalSpotOffsetUnits + ((i - 1) * UnitsPerPortalSpot), 0, lineLengthUnits - 1);
                Vector3 dashParticlePosition = dashParticlesArray[portalSpotIndex].transform.position;
                worldElectricTrailRendererPositions[i] = new Vector3(dashParticlePosition.x, dashParticlePosition.y + TrailRendererYOffset, dashParticlePosition.z);
            }

            DashParticlesItems.electricTrailRenderer.OverwritePositions(worldElectricTrailRendererPositions);

        }

        private void UpdateDashParticlesItems(int lineLengthUnits, float fillProgress)
        {
            bool activePassed = false;
            DashParticles[] dashParticlesArray = DashParticlesItems.dashParticles;

            if (fillProgress > 0.1f)
            {
                for (int i = 0; i < lineLengthUnits; i++)
                {
                    float lineLengthPercentage = (float)i / LineLengthUnits;
                    (bool active, float opacity) = CalculateDashParticlesOpacity(fillProgress, lineLengthPercentage);
                    DashParticles dashParticles = dashParticlesArray[i];
                    if (dashParticles.gameObject.activeSelf != active)
                    {
                        dashParticles.gameObject.SetActive(active);
                    }
                    if (active)
                    {
                        if (!activePassed)
                        {
                            activePassed = true;
                        }
                    }
                    else
                    {
                        if (activePassed)
                        {
                            break;
                        }
                    }
                }
            }

            activePassed = false;
            bool[] portalSpotsPassed = DashParticlesItems.portalSpotsPassed;
            for (int i = portalSpotsPassed.Length - 1; i >= 0; i--)
            {
                int particlesIndex = Math.Clamp(PortalSpotOffsetUnits + (i * UnitsPerPortalSpot), 0, lineLengthUnits - 1);
                float lineLengthPercentage = (float)particlesIndex / LineLengthUnits;

                (bool active, float opacity) = CalculateDashParticlesOpacity(fillProgress, lineLengthPercentage);

                if (active && !portalSpotsPassed[i])
                {
                    if (DashParticlesItems.lastArcPathsIndex < i)
                    {
                        Vector3 portalSpotPosition = dashParticlesArray[particlesIndex].transform.position;
                        DashParticlesItems.electricTrailRenderer.transform.position = new Vector3(portalSpotPosition.x,
                            portalSpotPosition.y + TrailRendererYOffset, portalSpotPosition.z);
                        DashParticlesItems.numElectricTrailRendererPositions = i + 2;
                        DashParticlesItems.lastArcPathsIndex = i;
                    }
                    portalSpotsPassed[i] = true;
                }

                if (!activePassed)
                {
                    if (active)
                    {
                        activePassed = true;
                    }
                }
                else
                {
                    break;
                }
            }

            //long elapsedTime = ElapsedTime;

            //PortalBuilderChain[] portalBuilderChains = DashParticlesItems.portalBuilderChains;
            ////if (timer > 1/3 * requiredDurations[0]) {}
            //for (int i = 0; i < playerClones.Length; i++)
            //{
            //    if (portalBuilderChains[i].UpdatePortals(elapsedTime))
            //    {
            //        break;
            //    }
            //}

            //CrimsonAuraBlack[] crimsonAurasArray = DashParticlesItems.crimsonAuras;
            //PortalOrbPurple[] portalOrbsArray = DashParticlesItems.portalOrbs;

            //activePassed = false;
            //for (int i = playerClones.Length - 1; i >= 0; i--)
            //{
            //    int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);
            //    float lineLengthPercentage = (float)particlesIndex / LineLengthUnits;

            //    (bool fullCloneOpacity, float cloneOpacity) = CalculatePlayerCloneOpacity(fillProgress, lineLengthPercentage);

            //    PlayerClientData playerCloneClientData = playerClones[i];
            //    PlayerComponent playerClone = playerCloneClientData.PlayerComponent;
            //    if (!activePassed)
            //    {
            //        if (fullCloneOpacity)
            //        {
            //            activePassed = true;
            //        }

            //        if (cloneOpacity > 0.2f)
            //        {
            //            playerClone.SetCloneFXOpacity(cloneOpacity);
            //            if (!playerClone.gameObject.activeSelf)
            //            {
            //                playerClone.gameObject.SetActive(true);
            //            }
            //            if (!playerClone.PlayerComponentCloneItems.AnimationStarted)
            //            {
            //                playerCloneClientData.PlayWalkingState();
            //                playerClone.PlayerComponentCloneItems.AnimationStarted = true;
            //            }
            //        }

            //    }
            //    else if (!playerClone.PlayerComponentCloneItems.AnimationTimerCompleted)
            //    {
            //        if (!playerClone.PlayerComponentCloneItems.AnimationTimerSet)
            //        {
            //            // temp, set based on hardcoded timer instead of motion duration...
            //            playerClone.PlayerComponentCloneItems.AnimationTimer.LastCheckedTime = Props.ObserverUpdateCache.UpdateTickTimeFixedUpdate;
            //            playerClone.PlayerComponentCloneItems.AnimationTimerSet = true;
            //        }
            //        else if (playerClone.PlayerComponentCloneItems.AnimationTimer.IsTimeElapsed_FixedUpdateThread())
            //        {
            //            playerClone.PlayerComponentCloneItems.AnimationTimerCompleted = true;
            //            playerClone.Animator.StopPlayback();
            //            playerClone.gameObject.SetActive(false);
            //        }
            //        else
            //        {
            //            float timerPercentage = playerClone.PlayerComponentCloneItems.AnimationTimer.RemainingDurationPercentage();
            //            playerClone.SetCloneFXOpacity(1f - timerPercentage);
            //            //Debug.Log($"{i}, {1f - timerPercentage}");
            //        }
            //    }
            //}
            
            // set opacity...
        }

        private (bool active, float opacity) CalculateDashParticlesOpacity(float fillProgress, float lineLengthPercentage)
        {
            if (fillProgress > 0.1f && fillProgress < 0.9f && lineLengthPercentage <= fillProgress)
            {
                float lineLengthDifference = fillProgress - lineLengthPercentage;

                if (lineLengthDifference < 0.28f)
                {
                    return (true, 1f - lineLengthDifference);
                }
            }
            return (false, 0f);
        }
        private (bool fullOpacity, float opacity) CalculatePlayerCloneOpacity(float fillProgress, float lineLengthPercentage)
        {
            if (fillProgress > 0.1f && fillProgress < 0.9f)
            {
                if (lineLengthPercentage >= fillProgress)
                {
                    return (false, Mathf.Min(0.4f, 1f - ((lineLengthPercentage - fillProgress) / 0.1f)));
                }
                else
                {
                    return (true, 0.4f);
                }
            }
            return (false, 0f);
        }
        private AnimationCurve CreateTerrainYVelocityAnimationCurve(float unitsPerKeyframe,
            float worldStartRotatedPositionX,
            float positionY,
            float worldStartRotatedPositionZ,
            float cosYAngle, float sinYAngle)
        {
            int numKeyframes = (int)Math.Ceiling(1f / unitsPerKeyframe);
            Keyframe[] keyframes = new Keyframe[numKeyframes];

            for (int i = 0; i < numKeyframes; i++)
            {
                float zAddUnits = unitsPerKeyframe * i;

                float localRotatedPositionX = zAddUnits * sinYAngle;
                float localRotatedPositionZ = zAddUnits * cosYAngle;

                float worldRotatedPositionX = worldStartRotatedPositionX + localRotatedPositionX;
                float worldRotatedPositionZ = worldStartRotatedPositionZ + localRotatedPositionZ;

                float newPositionY = Props.SkillAndAttackIndicatorSystem.GetTerrainHeight(worldRotatedPositionX, worldRotatedPositionZ);

                keyframes[i] = new Keyframe(i * unitsPerKeyframe, newPositionY - positionY);

                positionY = newPositionY;
            }

            return new AnimationCurve(keyframes);
        }
        //private float EaseInOutExpo(float percentage)
        //{
        //    return percentage < 0.5f ? (float)Math.Pow(2, 20 * percentage - 10) / 2 :
        //        (2 - (float)Math.Pow(2, -20 * percentage + 10)) / 2;
        //}
        private float EaseInOutQuad(float percentage)
        {
            return percentage * percentage;
        }
        private Vector3 GetTerrainProjectorPosition()
        {
            if (Props.SkillAndAttackIndicatorSystem.PlayerComponent != null)
            {
                Vector3 playerPosition = Props.SkillAndAttackIndicatorSystem.PlayerComponent.transform.position;
                return new Vector3(playerPosition.x, playerPosition.y + 50f, playerPosition.z);
            }
            else
            {
                //if (EventSystem.current.IsPointerOverGameObject() || !_isMouseOverGameWindow || IsPointerOverUIElement(GetEventSystemRaycastResults()))
                //    return _anchorPoint.transform.position;
                Ray ray = Props.SkillAndAttackIndicatorSystem.Camera.ScreenPointToRay(Input.mousePosition);
                return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Props.SkillAndAttackIndicatorSystem.TerrainLayer) ?
                    hit.point + new Vector3(0, 50, 0)
                    : new Vector3(0, 50, 0);
            }
        }
        public void TriggerDoubleCast()
        {
            throw new NotImplementedException();
        }


        // Removed Portal/Clone/Arc/ShockAura/CrimsonAura code
        //private static readonly int ArcPathFromSkyPerClone = 1;
        //private static readonly float ArcPathFromSkyPerCloneFloat = (float)ArcPathFromSkyPerClone;
        //private static readonly float ArcPathFromSkyRadius = 0.5f;
        //private static readonly float ArcPathZUnitsPerCluster = 1f;
        //private static readonly float ShockAuraYOffset = 0.7f;
        //PoolBagDco<AbstractAbilityFX> arcPathInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.ArcPath];
        //PoolBagDco<AbstractAbilityFX> shockAuraInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.ShockAura];
        //PoolBagDco<AbstractAbilityFX> crimsonAuraInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.CrimsonAuraBlack];
        //PoolBagDco<AbstractAbilityFX> portalOrbInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.PortalOrbPurple];
        //PoolBagDco<AbstractAbilityFX> portalBuilderSrcInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.PortalBuilder_Source];
        //PoolBagDco<AbstractAbilityFX> portalBuilderDestInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypeInstancePools.PortalBuilder_Dest];

        //long[] timeRequiredForDistancesPerUnit = TimeRequiredForDistancesPerUnit;

        //float arcLocalZStart = -1 * 0.5f * ArcPathZUnitsPerCluster;
        //float arcLocalZUnitsPerIndex = ArcPathZUnitsPerCluster / ArcPathFromSkyPerClone;
        //// cached prev values
        //int particlesIndex = Math.Clamp(CloneOffsetUnits, 0, lineLengthUnits - 1);
        //long prevTimeRequired = timeRequiredForDistancesPerUnit[particlesIndex];
        //long prevTimeRequiredAccum = prevTimeRequired;

        //for (int i = 0; i < numPlayerClones; i++)
        //{
        //    // Ensure the index is clamped to avoid approx error...
        //    int nextParticlesIndex = Math.Clamp(CloneOffsetUnits + ((i + 1) * UnitsPerClone), 0, lineLengthUnits - 1);
        //    long nextTimeRequired = timeRequiredForDistancesPerUnit[nextParticlesIndex] - prevTimeRequiredAccum;

        //    Vector3 dashParticlesPosition = dashParticles[particlesIndex].transform.position;
        //    PlayerComponent playerComponentClone = PlayerCloneInstancePool.InstantiatePooled(dashParticlesPosition);

        //    playerComponentClone.gameObject.transform.localEulerAngles = yRotationVector;
        //    playerComponentClone.OnCloneFXInit(Props.ObserverUpdateCache);
        //    playerComponentClone.gameObject.SetActive(false);
        //    PlayerClientData playerClientData = new PlayerClientData(playerComponentClone);
        //    playerClones[i] = playerClientData;

        //    for (int j = 0; j < ArcPathFromSkyPerClone; j++)
        //    {
        //        //int quadrant = (int) (j / ArcPathFromSkyPerCloneFloat * 4f);

        //        //int rangeStart = quadrant * 90 + 25;
        //        //int rangeEnd = (quadrant + 1) * 90 - 25;

        //        //int randomRotationY = Random.Next(rangeStart, rangeEnd);

        //        //float localPositionX = (float)Math.Sin(randomRotationY * Mathf.Deg2Rad) * ArcPathFromSkyRadius;
        //        //float localPositionZ = (float)Math.Cos(randomRotationY * Mathf.Deg2Rad) * ArcPathFromSkyRadius;

        //        //float rotatedLocalPositionX = localPositionZ * sinYAngle + localPositionX * cosYAngle;
        //        //float rotatedLocalPositionZ = localPositionZ * cosYAngle - localPositionX * sinYAngle;

        //        //ArcPath_Small_Floating arcPath = (ArcPath_Small_Floating)arcPathSmallFloatingInstancePool.InstantiatePooled(new Vector3(dashParticlesPosition.x + rotatedLocalPositionX,
        //        //    dashParticlesPosition.y,
        //        //    dashParticlesPosition.z + rotatedLocalPositionZ));

        //        //arcPath.transform.localEulerAngles = new Vector3(-15f, randomRotationY + yRotation, 0f);
        //        //arcPath.gameObject.SetActive(false);

        //        //arcPath.SetLocalPositionFields(
        //        //    localPositionX: localPositionX,
        //        //    localPositionZ: localPositionZ,
        //        //    localRotationY: randomRotationY);
        //        ArcPath arcPath = (ArcPath)arcPathInstancePool.InstantiatePooled(dashParticlesPosition);
        //        arcPath.transform.localEulerAngles = yRotationVector;
        //        arcPath.gameObject.SetActive(false);

        //        //float yStartOffset = Random.Next(-250, -239) * 0.01f;
        //        //float yEndOffset = Random.Next(20, 51) * 0.1f;
        //        //arcPath.SetOffsetFX(yStartOffset, yEndOffset);

        //        arcPathsFromSky[(i * ArcPathFromSkyPerClone) + j] = arcPath;
        //    }

        //    ShockAura shockAura = (ShockAura)shockAuraInstancePool.InstantiatePooled(new Vector3(dashParticlesPosition.x,
        //        dashParticlesPosition.y + ShockAuraYOffset, dashParticlesPosition.z));
        //    shockAura.transform.localEulerAngles = yRotationVector;
        //    shockAura.gameObject.SetActive(false);

        //    shockAuras[i] = shockAura;

        //    CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack) crimsonAuraInstancePool.InstantiatePooled(null);
        //    // emit is changed instead of using active.

        //    crimsonAuras[i] = crimsonAura;

        //    PortalOrbPurple portalOrb = (PortalOrbPurple)portalOrbInstancePool.InstantiatePooled(null);
        //    // emit is changed instead of using active.

        //    portalOrbs[i] = portalOrb;

        //    //src durationAllowed =  1/3 * nextTimeDelay
        //    PortalBuilder portalSource = (PortalBuilder)portalBuilderSrcInstancePool.InstantiatePooled(dashParticlesPosition);
        //    portalSource.transform.localEulerAngles = yRotationVector;
        //    portalSource.gameObject.SetActive(false);
        //    long endPortalTimeOffset = (long)(SkillAndAttackIndicatorSystem.ONE_THIRD * nextTimeRequired);
        //    portalSource.Initialize(Props.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, endPortalTimeOffset, 
        //        setPlayerInactive: true, isClone: true);

        //    //dest durationAllowed = 2/3 * prevTimeDelay
        //    PortalBuilder portalDest = (PortalBuilder)portalBuilderDestInstancePool.InstantiatePooled(dashParticlesPosition);
        //    portalDest.transform.localEulerAngles = yRotationVector;
        //    portalDest.gameObject.SetActive(false);
        //    long startPortalTimeOffset = (long)(SkillAndAttackIndicatorSystem.TWO_THIRDS * prevTimeRequired);
        //    portalDest.Initialize(Props.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, startPortalTimeOffset,
        //        setPlayerInactive: true, isClone: true);

        //    //Debug.Log($"time required at {nextParticlesIndex}: {timeRequiredForDistancesPerUnit[nextParticlesIndex]}");
        //    //Debug.Log($"{i}, {prevTimeRequired}, {nextTimeRequired}");
        //    PortalBuilderChain portalBuilderChain = new PortalBuilderChain(portalSource, portalDest,
        //        startTime: prevTimeRequiredAccum - startPortalTimeOffset,
        //        endTime: prevTimeRequiredAccum + endPortalTimeOffset,
        //        inverted: true);
        //    portalBuilderChains[i] = portalBuilderChain;

        //    particlesIndex = nextParticlesIndex;
        //    prevTimeRequired = nextTimeRequired;
        //    prevTimeRequiredAccum += nextTimeRequired;
        //}
        //UpdatePos:
        //PlayerClientData[] playerClonesArray = DashParticlesItems.playerClones;
        //ArcPath[] arcPathsFromSkyArray = DashParticlesItems.arcPathsFromSky;
        //ShockAura[] shockAurasArray = DashParticlesItems.shockAuras;
        //PortalBuilderChain[] portalBuilderChains = DashParticlesItems.portalBuilderChains;

        //for (int i = 0; i < playerClonesArray.Length; i++)
        //{
        //    // Ensure the index is clamped to avoid approx error...
        //    int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);

        //    Transform playerCloneTransform = playerClonesArray[i].PlayerComponent.transform;

        //    Vector3 dashParticlesPosition = dashParticlesArray[particlesIndex].transform.position;
        //    playerCloneTransform.position = dashParticlesPosition;
        //    playerCloneTransform.localEulerAngles = yRotationVector;

        //    for (int j = 0; j < ArcPathFromSkyPerClone; j++)
        //    {
        //        ArcPath arcPath = DashParticlesItems.arcPathsFromSky[(i * ArcPathFromSkyPerClone) + j];

        //        Transform arcPathsTransform = arcPathsFromSkyArray[(i * ArcPathFromSkyPerClone) + j].transform;

        //        arcPathsTransform.position = dashParticlesPosition;
        //        arcPathsTransform.localEulerAngles = yRotationVector;
        //    }

        //    Transform shockAuraTransform = shockAurasArray[i].transform;
        //    shockAuraTransform.position = new Vector3(dashParticlesPosition.x, dashParticlesPosition.y + ShockAuraYOffset, dashParticlesPosition.z);
        //    shockAuraTransform.localEulerAngles = yRotationVector;

        //    PortalBuilderChain portalBuilderChain = portalBuilderChains[i];
        //    Transform portalSrcTransform = portalBuilderChain.PortalSource.transform;
        //    portalSrcTransform.position = dashParticlesPosition;
        //    portalSrcTransform.localEulerAngles = yRotationVector;

        //    Transform portalDestTransform = portalBuilderChain.PortalDest.transform;
        //    portalDestTransform.position = dashParticlesPosition;
        //    portalDestTransform.localEulerAngles = yRotationVector;
        //}
    }

    public enum PlayerComponentModel
    {
        Starter
    }
    public enum AbilityProjectorType
    {
        Arc,
        Circle,
        Line,
        ScatterLine,
    }
    public enum AbilityProjectorMaterialType
    {
        First
    }
    public enum AbilityIndicatorCastType
    {
        ShowDuringCast,
        DoubleCast
    }
    public enum AbilityIndicatorFXType
    {
        None,
        DashParticles
    }
    public enum DashParticlesFXTypeInstancePools
    {
        DashParticles,
        ElectricTrailRenderer
    }
    public enum AbilityFXComponentType
    {
        DashParticles,
        ArcPath,
        ArcPath_Small_Floating,
        ElectricTrail,
        ElectricTrailRenderer,
        ShockAura,
        CrimsonAuraBlack,
        PortalOrbPurple,
        PortalBuilder_Source,
        PortalBuilder_Dest
    }
}
