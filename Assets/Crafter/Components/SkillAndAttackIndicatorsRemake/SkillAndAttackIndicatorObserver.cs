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

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class SkillAndAttackIndicatorObserverProps
    {
        public SkillAndAttackIndicatorSystem SkillAndAttackIndicatorSystem;

        public ObserverUpdateProps ObserverUpdateProps;
        public SkillAndAttackIndicatorObserverProps(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, ObserverUpdateProps observerUpdateProps)
        {
            SkillAndAttackIndicatorSystem = skillAndAttackIndicatorSystem;
            ObserverUpdateProps = observerUpdateProps;
        }
    }
    public class SkillAndAttackIndicatorObserver
    {
        public static readonly string[] AbilityProjectorTypeNames = Enum.GetNames(typeof(AbilityProjectorType));
        public static readonly int AbilityProjectorTypeNamesLength = AbilityProjectorTypeNames.Length;
        public static readonly string[] AbilityProjectorMaterialTypeNames = Enum.GetNames(typeof(AbilityProjectorMaterialType));
        public static readonly int AbilityProjectorMaterialTypeNamesLength = AbilityProjectorMaterialTypeNames.Length;
        public static readonly string[] AbilityFXTypeNames = Enum.GetNames(typeof(AbilityFXType));
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
        private static readonly int CloneOffsetUnits = 2;
        private static readonly int UnitsPerClone = 5;
        private static readonly int ArcPathFromSkyPerClone = 5;
        private static readonly float ArcPathZUnitsPerCluster = 1f;

        private SkillAndAttackIndicatorObserverProps Props;

        public ObserverStatus ObserverStatus = ObserverStatus.Active;

        public readonly AbilityProjectorType AbilityProjectorType;
        public readonly AbilityProjectorMaterialType AbilityProjectorMaterialType;
        public readonly AbilityIndicatorCastType AbilityIndicatorCastType;
        public readonly AbilityFXType[] AbilityFXTypes;
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

        private (DashParticles[] dashParticles, PlayerComponent[] playerClones,
            ArcPath[] arcPaths) DashParticlesItems;

        private long ChargeDuration;
        private float ChargeDurationSecondsFloat;

        private Vector3 PreviousTerrainProjectorPosition;
        private float PreviousRotationY;
        private float PreviousChargeDurationFloatPercentage;

        private long LastTickTime;
        private long ElapsedTime;
        private float ElapsedTimeSecondsFloat;

        public SkillAndAttackIndicatorObserver(AbilityProjectorType abilityProjectorType,
            AbilityProjectorMaterialType abilityProjectorMaterialType, AbilityIndicatorCastType abilityIndicatorCastType,
            AbilityFXType[] abilityFXTypes,
            SkillAndAttackIndicatorObserverProps skillAndAttackIndicatorObserverProps
            )
        {
            AbilityProjectorType = abilityProjectorType;
            AbilityProjectorMaterialType = abilityProjectorMaterialType;
            AbilityIndicatorCastType = abilityIndicatorCastType;
            AbilityFXTypes = abilityFXTypes;
            PlayerGuid = skillAndAttackIndicatorObserverProps.SkillAndAttackIndicatorSystem.PlayerGuid;

            Props = skillAndAttackIndicatorObserverProps;
        }

        public void OnUpdate()
        {
            if (!ProjectorSet)
            {
                if (Props.SkillAndAttackIndicatorSystem.ProjectorInstancePools.TryGetValue(AbilityProjectorType, out var abilityMaterialTypesDict) &&
                    abilityMaterialTypesDict.TryGetValue(AbilityProjectorMaterialType, out ProjectorInstancePool) &&
                    (AbilityFXTypes == null ||
                    (Props.SkillAndAttackIndicatorSystem.AbilityFXInstancePools.TryGetValuesAll(AbilityFXTypes, out AbilityFXInstancePools) &&
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

                    if (AbilityFXTypes != null)
                    {
                        for (int i = 0; i < AbilityFXTypes.Length; i++)
                        {
                            AbilityFXType abilityFXType = AbilityFXTypes[i];
                            switch (abilityFXType)
                            {
                                case AbilityFXType.DashParticles:
                                    DashParticlesItems = CreateDashParticlesItems(LineLengthUnits,
                                        terrainProjectorPosition.x, terrainProjectorPosition.z, GetThirdPersonControllerRotation(),
                                        i);
                                    break;
                            }
                        }
                    }
                    
                    switch (AbilityProjectorMaterialType)
                    {
                        case AbilityProjectorMaterialType.First:
                            ChargeDuration = 800L;
                            ChargeDurationSecondsFloat = 800 * 0.001f;
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

                    ProjectorSet = true;
                    LastTickTime = Props.ObserverUpdateProps.UpdateTickTimeFixedUpdate;
                }
                else
                {
                    ObserverStatus = ObserverStatus.Remove;
                    return;
                }
            }
            else
            {
                long elapsedTickTime = Props.ObserverUpdateProps.UpdateTickTimeFixedUpdate - LastTickTime;
                LastTickTime = Props.ObserverUpdateProps.UpdateTickTimeFixedUpdate;
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
                                newFillProgress = EaseInOutExpo(chargeDurationPercentage);
                                LineRegionProjectorRef.FillProgress = newFillProgress;
                                LineRegionProjectorRef.UpdateProjectors();
                                PreviousChargeDurationFloatPercentage = chargeDurationPercentage;
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

                if (AbilityFXTypes != null)
                {
                    float rotationDifference = playerRotation - PreviousRotationY;
                    if (rotationDifference < -10f || rotationDifference > 10f ||
                        (PreviousTerrainProjectorPosition - terrainProjectorPosition).magnitude > 0.03f)
                    {
                        foreach (AbilityFXType abilityFXType in AbilityFXTypes)
                        {
                            switch (abilityFXType)
                            {
                                case AbilityFXType.DashParticles:
                                    UpdateDashParticlesItems(LineLengthUnits, terrainProjectorPosition.x, terrainProjectorPosition.z, playerRotation,
                                        fillProgress: newFillProgress);
                                    break;
                            }
                        }
                        
                        PreviousTerrainProjectorPosition = terrainProjectorPosition;
                        PreviousRotationY = playerRotation;
                    }

                    foreach (AbilityFXType abilityFXType in AbilityFXTypes)
                    {
                        switch (abilityFXType)
                        {
                            case AbilityFXType.DashParticles:
                                UpdateDashParticlesOpacities(LineLengthUnits, newFillProgress);
                                break;
                        }
                    }
                    // every update
                }
            }

            if (ElapsedTime > ChargeDuration)
            {
                ProjectorInstancePool.ReturnPooled(ProjectorMonoBehaviour);
                if (AbilityFXTypes != null)
                {
                    for (int i = 0; i < AbilityFXTypes.Length; i++)
                    {
                        PoolBagDco<AbstractAbilityFX>[] abilityFXInstancePool = AbilityFXInstancePools[i];
                        switch (AbilityFXTypes[i])
                        {
                            case AbilityFXType.DashParticles:
                                PoolBagDco<AbstractAbilityFX> dashParticlesPool = abilityFXInstancePool[0];
                                PoolBagDco<AbstractAbilityFX> arcPathPool = abilityFXInstancePool[1];
                                foreach (DashParticles dashParticles in DashParticlesItems.dashParticles)
                                {
                                    dashParticlesPool.ReturnPooled(dashParticles);
                                }
                                foreach (PlayerComponent playerClone in DashParticlesItems.playerClones)
                                {
                                    PlayerCloneInstancePool.ReturnPooled(playerClone);
                                }
                                foreach (ArcPath arcPath in DashParticlesItems.arcPaths)
                                {
                                    arcPathPool.ReturnPooled(arcPath);
                                }
                                break;
                        }
                    }
                }

                ObserverStatus = ObserverStatus.Remove;
            }
        }

        private float GetThirdPersonControllerRotation()
        {
            if (Props.SkillAndAttackIndicatorSystem.PlayerComponent != null)
            {
                return Props.SkillAndAttackIndicatorSystem.PlayerComponent.transform.localEulerAngles.y;
            }
            return 0f;
        }

        private (DashParticles[] dashParticles,
            PlayerComponent[] playerClones,
            ArcPath[] arcPaths
            ) CreateDashParticlesItems(int lineLengthUnits,
            float startPositionX, float startPositionZ,
            float yRotation, int abilityFXIndex)
        {
            int numPlayerClones = (int)Math.Floor((lineLengthUnits - CloneOffsetUnits) / (float)UnitsPerClone);

            DashParticles[] dashParticles = new DashParticles[lineLengthUnits];
            PlayerComponent[] playerClones = new PlayerComponent[numPlayerClones];
            ArcPath[] arcPaths = new ArcPath[numPlayerClones * ArcPathFromSkyPerClone];

            float cosYAngle = (float)Math.Cos(yRotation * Mathf.Deg2Rad);
            float sinYAngle = (float)Math.Sin(yRotation * Mathf.Deg2Rad);

            Vector3 yRotationVector = new Vector3(0f, yRotation, 0f);

            float worldRotatedPositionX = startPositionX;
            float worldRotatedPositionZ = startPositionZ;

            PoolBagDco<AbstractAbilityFX> dashParticlesInstancePool = AbilityFXInstancePools[abilityFXIndex][0];

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

            for (int i = 0; i < numPlayerClones; i++)
            {
                // Ensure the index is clamped to avoid approx error...
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);

                PlayerComponent playerComponentClone = PlayerCloneInstancePool.InstantiatePooled(dashParticles[particlesIndex].transform.position);
                playerComponentClone.gameObject.transform.localEulerAngles = yRotationVector;
                playerComponentClone.SetCloneFX();
                playerComponentClone.gameObject.SetActive(false);
                playerClones[i] = playerComponentClone;
            }

            PoolBagDco<AbstractAbilityFX> arcPathInstancePool = AbilityFXInstancePools[abilityFXIndex][1];
            
            float arcLocalZStart = -1 * 0.5f * ArcPathZUnitsPerCluster;
            float arcLocalZUnitsPerIndex = ArcPathZUnitsPerCluster / ArcPathFromSkyPerClone;
            for (int i = 0; i < numPlayerClones; i++)
            {
                // Ensure the index is clamped to avoid approx error...
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);

                Vector3 dashParticlesPosition = dashParticles[particlesIndex].transform.position;

                for (int j = 0; j < ArcPathFromSkyPerClone; j++)
                {
                    // alternate side with 2 vs 1
                    float localPositionX;
                    float localPositionZ = arcLocalZStart + (j * arcLocalZUnitsPerIndex);
                    if ((i + j) % 2 == 0)
                    {
                        localPositionX = (Random.Next(-35, -14) + Random.Next(-3, 1)) * 0.01f;
                    }
                    else
                    {
                        localPositionX = (Random.Next(15, 36) + Random.Next(0, 4)) * 0.01f;
                    }

                    float rotatedLocalPositionX = localPositionZ * sinYAngle + localPositionX * cosYAngle;
                    float rotatedLocalPositionZ = localPositionZ * cosYAngle - localPositionX * sinYAngle;

                    ArcPath arcPath = (ArcPath)arcPathInstancePool.InstantiatePooled(new Vector3(dashParticlesPosition.x + rotatedLocalPositionX,
                        dashParticlesPosition.y,
                        dashParticlesPosition.z + rotatedLocalPositionZ));
                    arcPath.transform.localEulerAngles = yRotationVector;
                    arcPath.gameObject.SetActive(false);

                    arcPath.SetLocalPositionFields(
                        localPositionX: localPositionX,
                        localPositionZ: localPositionZ);

                    arcPaths[(i * ArcPathFromSkyPerClone) + j] = arcPath;
                }
            }

            return (dashParticles, playerClones, arcPaths);
        }
        private void UpdateDashParticlesItems(int lineLengthUnits,
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

            PlayerComponent[] playerClonesArray = DashParticlesItems.playerClones;
            for (int i = 0; i < playerClonesArray.Length; i++)
            {
                // Ensure the index is clamped to avoid approx error...
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);

                Transform playerCloneTransform = playerClonesArray[i].transform;

                playerCloneTransform.position = dashParticlesArray[particlesIndex].transform.position;
                playerCloneTransform.localEulerAngles = yRotationVector;
            }

            for (int i = 0; i < playerClonesArray.Length; i++)
            {
                // Ensure the index is clamped to avoid approx error...
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);

                Vector3 dashParticlesPosition = dashParticlesArray[particlesIndex].transform.position;

                ArcPath[] arcPathsArray = DashParticlesItems.arcPaths;

                for (int j = 0; j < ArcPathFromSkyPerClone; j++)
                {
                    ArcPath arcPath = DashParticlesItems.arcPaths[(i * ArcPathFromSkyPerClone) + j];

                    Transform arcPathsTransform = arcPathsArray[(i * ArcPathFromSkyPerClone) + j].transform;

                    float rotatedLocalPositionX = arcPath.LocalPositionZ * sinYAngle + arcPath.LocalPositionX * cosYAngle;
                    float rotatedLocalPositionZ = arcPath.LocalPositionZ * cosYAngle - arcPath.LocalPositionX * sinYAngle;

                    arcPathsTransform.position = new Vector3(dashParticlesPosition.x + rotatedLocalPositionX, 
                        dashParticlesPosition.y, 
                        dashParticlesPosition.z + rotatedLocalPositionZ);

                    arcPathsTransform.localEulerAngles = yRotationVector;

                }
            }
        }

        private void UpdateDashParticlesOpacities(int lineLengthUnits, float fillProgress)
        {
            bool activePassed = false;
            DashParticles[] dashParticlesArray = DashParticlesItems.dashParticles;
            for (int i = 0; i < lineLengthUnits; i++)
            {
                (bool active, float opacity) = CalculateDashParticlesOpacity(fillProgress, i);
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

            activePassed = false;
            PlayerComponent[] playerClones = DashParticlesItems.playerClones;
            ArcPath[] arcPaths = DashParticlesItems.arcPaths;
            for (int i = 0; i < playerClones.Length; i++)
            {
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);
                (bool active, float opacity) = CalculateDashParticlesOpacity(fillProgress, particlesIndex);
                PlayerComponent playerClone = playerClones[i];
                if (playerClone.gameObject.activeSelf != active)
                {
                    playerClone.gameObject.SetActive(active);
                    for (int j = 0; j < ArcPathFromSkyPerClone; j++)
                    {
                        ArcPath arcPath = arcPaths[(i * ArcPathFromSkyPerClone) + j];
                        arcPath.gameObject.SetActive(active);
                    }
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
            
            // set opacity...
        }

        private (bool active, float opacity) CalculateDashParticlesOpacity(float fillProgress, int lineLengthIndex)
        {
            float lineLengthPercentage = (float) lineLengthIndex / LineLengthUnits;

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
        private float EaseInOutExpo(float percentage)
        {
            return percentage < 0.5f ? (float)Math.Pow(2, 20 * percentage - 10) / 2 :
                (2 - (float)Math.Pow(2, -20 * percentage + 10)) / 2;
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
    }

    public enum ObserverStatus
    {
        Active,
        Remove
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
    public enum AbilityFXType
    {
        None,
        DashParticles
    }
    public enum AbilityFXComponentType
    {
        DashParticles,
        ArcPath,
    }
}
