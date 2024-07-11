﻿using System;
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
        // Also used for adding ArcPathFromCloneOffset
        private static readonly int CloneOffsetUnits = 2;
        private static readonly int UnitsPerClone = 5;
        private static readonly int ArcPathFromSkyPerClone = 1;
        private static readonly float ArcPathFromSkyPerCloneFloat = (float)ArcPathFromSkyPerClone;
        private static readonly float ArcPathFromSkyRadius = 0.5f;
        private static readonly float ArcPathZUnitsPerCluster = 1f;
        private static readonly float ShockAuraYOffset = 0.7f;
        private static readonly float CrimsonAuraYOffset = 0.85f;
        private static readonly float PortalOrbYOffset = 0.7f;
        private static readonly float TrailRendererYOffset = 0.7f;
        

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

        private (DashParticles[] dashParticles, PlayerClientData[] playerClones,
            ArcPath[] arcPathsFromSky, 
            ShockAura[] shockAuras,
            CrimsonAuraBlack[] crimsonAuras,
            PortalOrbPurple[] portalOrbs,
            ElectricTrailRenderer electricTrailRenderer,
            int numElectricTrailRendererPositions,
            int lastArcPathsIndex) DashParticlesItems;

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
                                    UpdateDashParticlesItemsPositions(LineLengthUnits, terrainProjectorPosition.x, terrainProjectorPosition.z, playerRotation,
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
                                UpdateDashParticlesItems(LineLengthUnits, newFillProgress);
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
                                PoolBagDco<AbstractAbilityFX> dashParticlesPool = abilityFXInstancePool[(int) DashParticlesFXTypePrefabPools.DashParticles];
                                PoolBagDco<AbstractAbilityFX> arcPathPool = abilityFXInstancePool[(int)DashParticlesFXTypePrefabPools.ArcPath];
                                PoolBagDco<AbstractAbilityFX> electricTrailRendererPool = abilityFXInstancePool[(int)DashParticlesFXTypePrefabPools.ElectricTrailRenderer];
                                PoolBagDco<AbstractAbilityFX> shockAuraPool = abilityFXInstancePool[(int)DashParticlesFXTypePrefabPools.ShockAura];
                                PoolBagDco<AbstractAbilityFX> crimsonAuraPool = abilityFXInstancePool[(int)DashParticlesFXTypePrefabPools.CrimsonAuraBlack];
                                PoolBagDco<AbstractAbilityFX> portalOrbPool = abilityFXInstancePool[(int)DashParticlesFXTypePrefabPools.PortalOrbPurple];

                                foreach (DashParticles dashParticles in DashParticlesItems.dashParticles)
                                {
                                    dashParticlesPool.ReturnPooled(dashParticles);
                                }
                                foreach (PlayerClientData playerClientData in DashParticlesItems.playerClones)
                                {
                                    playerClientData.PlayerComponent.Animator.StopPlayback();
                                    PlayerCloneInstancePool.ReturnPooled(playerClientData.PlayerComponent);
                                }
                                foreach (ArcPath arcPath in DashParticlesItems.arcPathsFromSky)
                                {
                                    arcPathPool.ReturnPooled(arcPath);
                                }
                                foreach (ShockAura shockAura in DashParticlesItems.shockAuras)
                                {
                                    shockAuraPool.ReturnPooled(shockAura);
                                }
                                foreach (CrimsonAuraBlack crimsonAura in DashParticlesItems.crimsonAuras)
                                {
                                    crimsonAuraPool.ReturnPooled(crimsonAura);
                                }
                                foreach (PortalOrbPurple portalOrb in DashParticlesItems.portalOrbs)
                                {
                                    portalOrbPool.ReturnPooled(portalOrb);
                                }

                                DashParticlesItems.electricTrailRenderer.ClearAll();
                                electricTrailRendererPool.ReturnPooled(DashParticlesItems.electricTrailRenderer);
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
            PlayerClientData[] playerClones,
            ArcPath[] arcPathsFromSky,
            ShockAura[] shockAuras,
            CrimsonAuraBlack[] crimsonAuras,
            PortalOrbPurple[] portalOrbs,
            ElectricTrailRenderer electricTrailRenderer,
            int numElectricTrailRendererPositions,
            int lastArcPathsIndex
            ) CreateDashParticlesItems(int lineLengthUnits,
            float startPositionX, float startPositionZ,
            float yRotation, int abilityFXIndex)
        {
            int numPlayerClones = (int)Math.Floor((lineLengthUnits - CloneOffsetUnits) / (float)UnitsPerClone);

            DashParticles[] dashParticles = new DashParticles[lineLengthUnits];
            PlayerClientData[] playerClones = new PlayerClientData[numPlayerClones];
            ArcPath[] arcPathsFromSky = new ArcPath[numPlayerClones * ArcPathFromSkyPerClone];
            ShockAura[] shockAuras = new ShockAura[numPlayerClones];
            CrimsonAuraBlack[] crimsonAuras = new CrimsonAuraBlack[numPlayerClones];
            PortalOrbPurple[] portalOrbs = new PortalOrbPurple[numPlayerClones];

            float cosYAngle = (float)Math.Cos(yRotation * Mathf.Deg2Rad);
            float sinYAngle = (float)Math.Sin(yRotation * Mathf.Deg2Rad);

            Vector3 yRotationVector = new Vector3(0f, yRotation, 0f);

            float worldRotatedPositionX = startPositionX;
            float worldRotatedPositionZ = startPositionZ;

            PoolBagDco<AbstractAbilityFX>[] dashParticlesTypeFXPools = AbilityFXInstancePools[abilityFXIndex];

            PoolBagDco<AbstractAbilityFX> dashParticlesInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypePrefabPools.DashParticles];

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

            PoolBagDco<AbstractAbilityFX> arcPathInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypePrefabPools.ArcPath];
            PoolBagDco<AbstractAbilityFX> shockAuraInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypePrefabPools.ShockAura];
            PoolBagDco<AbstractAbilityFX> crimsonAuraInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypePrefabPools.CrimsonAuraBlack];
            PoolBagDco<AbstractAbilityFX> portalOrbInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypePrefabPools.PortalOrbPurple];

            float arcLocalZStart = -1 * 0.5f * ArcPathZUnitsPerCluster;
            float arcLocalZUnitsPerIndex = ArcPathZUnitsPerCluster / ArcPathFromSkyPerClone;

            for (int i = 0; i < numPlayerClones; i++)
            {
                // Ensure the index is clamped to avoid approx error...
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);

                Vector3 dashParticlesPosition = dashParticles[particlesIndex].transform.position;
                PlayerComponent playerComponentClone = PlayerCloneInstancePool.InstantiatePooled(dashParticlesPosition);

                playerComponentClone.gameObject.transform.localEulerAngles = yRotationVector;
                playerComponentClone.OnCloneFXInit(Props.ObserverUpdateCache);
                playerComponentClone.gameObject.SetActive(false);
                playerClones[i] = new PlayerClientData(playerComponentClone);

                for (int j = 0; j < ArcPathFromSkyPerClone; j++)
                {
                    //int quadrant = (int) (j / ArcPathFromSkyPerCloneFloat * 4f);

                    //int rangeStart = quadrant * 90 + 25;
                    //int rangeEnd = (quadrant + 1) * 90 - 25;

                    //int randomRotationY = Random.Next(rangeStart, rangeEnd);

                    //float localPositionX = (float)Math.Sin(randomRotationY * Mathf.Deg2Rad) * ArcPathFromSkyRadius;
                    //float localPositionZ = (float)Math.Cos(randomRotationY * Mathf.Deg2Rad) * ArcPathFromSkyRadius;

                    //float rotatedLocalPositionX = localPositionZ * sinYAngle + localPositionX * cosYAngle;
                    //float rotatedLocalPositionZ = localPositionZ * cosYAngle - localPositionX * sinYAngle;

                    //ArcPath_Small_Floating arcPath = (ArcPath_Small_Floating)arcPathSmallFloatingInstancePool.InstantiatePooled(new Vector3(dashParticlesPosition.x + rotatedLocalPositionX,
                    //    dashParticlesPosition.y,
                    //    dashParticlesPosition.z + rotatedLocalPositionZ));

                    //arcPath.transform.localEulerAngles = new Vector3(-15f, randomRotationY + yRotation, 0f);
                    //arcPath.gameObject.SetActive(false);

                    //arcPath.SetLocalPositionFields(
                    //    localPositionX: localPositionX,
                    //    localPositionZ: localPositionZ,
                    //    localRotationY: randomRotationY);
                    ArcPath arcPath = (ArcPath)arcPathInstancePool.InstantiatePooled(dashParticlesPosition);
                    arcPath.transform.localEulerAngles = yRotationVector;
                    arcPath.gameObject.SetActive(false);

                    //float yStartOffset = Random.Next(-250, -239) * 0.01f;
                    //float yEndOffset = Random.Next(20, 51) * 0.1f;
                    //arcPath.SetOffsetFX(yStartOffset, yEndOffset);

                    arcPathsFromSky[(i * ArcPathFromSkyPerClone) + j] = arcPath;
                }

                ShockAura shockAura = (ShockAura)shockAuraInstancePool.InstantiatePooled(new Vector3(dashParticlesPosition.x,
                    dashParticlesPosition.y + ShockAuraYOffset, dashParticlesPosition.z));
                shockAura.transform.localEulerAngles = yRotationVector;
                shockAura.gameObject.SetActive(false);

                shockAuras[i] = shockAura;

                CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack) crimsonAuraInstancePool.InstantiatePooled(new Vector3(dashParticlesPosition.x,
                    dashParticlesPosition.y + CrimsonAuraYOffset, dashParticlesPosition.z));
                crimsonAura.transform.localEulerAngles = yRotationVector;
                crimsonAura.gameObject.SetActive(false);

                crimsonAuras[i] = crimsonAura;

                PortalOrbPurple portalOrb = (PortalOrbPurple)portalOrbInstancePool.InstantiatePooled(new Vector3(dashParticlesPosition.x,
                    dashParticlesPosition.y + PortalOrbYOffset, dashParticlesPosition.z));
                portalOrb.transform.localEulerAngles = yRotationVector;
                portalOrb.gameObject.SetActive(false);

                portalOrbs[i] = portalOrb;
            }

            PoolBagDco<AbstractAbilityFX> electricTrailRendererInstancePool = dashParticlesTypeFXPools[(int)DashParticlesFXTypePrefabPools.ElectricTrailRenderer];

            ElectricTrailRenderer electricTrailRenderer = (ElectricTrailRenderer) electricTrailRendererInstancePool.InstantiatePooled(dashParticles[0].transform.position);

            return (dashParticles, playerClones, arcPathsFromSky, shockAuras, crimsonAuras, portalOrbs, electricTrailRenderer, 1, -1);
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

            PlayerClientData[] playerClonesArray = DashParticlesItems.playerClones;
            ArcPath[] arcPathsFromSkyArray = DashParticlesItems.arcPathsFromSky;
            ShockAura[] shockAurasArray = DashParticlesItems.shockAuras;
            CrimsonAuraBlack[] crimsonAurasArray = DashParticlesItems.crimsonAuras;
            PortalOrbPurple[] portalOrbs = DashParticlesItems.portalOrbs;

            for (int i = 0; i < playerClonesArray.Length; i++)
            {
                // Ensure the index is clamped to avoid approx error...
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);

                Transform playerCloneTransform = playerClonesArray[i].PlayerComponent.transform;

                Vector3 dashParticlesPosition = dashParticlesArray[particlesIndex].transform.position;
                playerCloneTransform.position = dashParticlesPosition;
                playerCloneTransform.localEulerAngles = yRotationVector;

                for (int j = 0; j < ArcPathFromSkyPerClone; j++)
                {
                    ArcPath arcPath = DashParticlesItems.arcPathsFromSky[(i * ArcPathFromSkyPerClone) + j];

                    Transform arcPathsTransform = arcPathsFromSkyArray[(i * ArcPathFromSkyPerClone) + j].transform;

                    arcPathsTransform.position = dashParticlesPosition;
                    arcPathsTransform.localEulerAngles = yRotationVector;
                }

                Transform shockAuraTransform = shockAurasArray[i].transform;
                shockAuraTransform.position = new Vector3(dashParticlesPosition.x, dashParticlesPosition.y + ShockAuraYOffset, dashParticlesPosition.z);
                shockAuraTransform.localEulerAngles = yRotationVector;

                Transform crimsonAuraTransform = crimsonAurasArray[i].transform;
                crimsonAuraTransform.position = new Vector3(dashParticlesPosition.x, dashParticlesPosition.y + CrimsonAuraYOffset, dashParticlesPosition.z);
                crimsonAuraTransform.localEulerAngles = yRotationVector;

                Transform portalOrbTransform = portalOrbs[i].transform;
                portalOrbTransform.position = new Vector3(dashParticlesPosition.x, dashParticlesPosition.y + PortalOrbYOffset, dashParticlesPosition.z);
                portalOrbTransform.localEulerAngles = yRotationVector;
            }

            Vector3[] worldElectricTrailRendererPositions = new Vector3[DashParticlesItems.numElectricTrailRendererPositions];
            worldElectricTrailRendererPositions[0] = dashParticlesArray[0].transform.position;
            
            for (int i = 1; i < worldElectricTrailRendererPositions.Length; i++)
            {
                Vector3 playerClonePosition = playerClonesArray[i - 1].PlayerComponent.transform.position;
                worldElectricTrailRendererPositions[i] = new Vector3(playerClonePosition.x, playerClonePosition.y + TrailRendererYOffset, playerClonePosition.z);
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
            PlayerClientData[] playerClones = DashParticlesItems.playerClones;
            ArcPath[] arcPathsFromSky = DashParticlesItems.arcPathsFromSky;
            ShockAura[] shockAurasArray = DashParticlesItems.shockAuras;
            CrimsonAuraBlack[] crimsonAurasArray = DashParticlesItems.crimsonAuras;
            PortalOrbPurple[] portalOrbsArray = DashParticlesItems.portalOrbs;
            for (int i = playerClones.Length - 1; i >= 0; i--)
            {
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);
                float lineLengthPercentage = (float)particlesIndex / LineLengthUnits;

                (bool active, float opacity) = CalculateDashParticlesOpacity(fillProgress, lineLengthPercentage);

                ArcPath arcPath = arcPathsFromSky[i * ArcPathFromSkyPerClone];

                if (arcPath.gameObject.activeSelf != active)
                {
                    arcPath.gameObject.SetActive(active);
                    shockAurasArray[i].gameObject.SetActive(active);
                    crimsonAurasArray[i].gameObject.SetActive(active);
                    portalOrbsArray[i].gameObject.SetActive(active);
                    for (int j = 1; j < ArcPathFromSkyPerClone; j++)
                    {
                        arcPath = arcPathsFromSky[(i * ArcPathFromSkyPerClone) + j];
                        arcPath.gameObject.SetActive(active);
                    }
                    if (active && DashParticlesItems.lastArcPathsIndex < i)
                    {
                        Vector3 playerClonePosition = playerClones[i].PlayerComponent.transform.position;
                        DashParticlesItems.electricTrailRenderer.transform.position = new Vector3(playerClonePosition.x,
                            playerClonePosition.y + TrailRendererYOffset, playerClonePosition.z);
                        DashParticlesItems.numElectricTrailRendererPositions = i + 2;
                        DashParticlesItems.lastArcPathsIndex = i;
                    }
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

            activePassed = false;
            for (int i = playerClones.Length - 1; i >= 0; i--)
            {
                int particlesIndex = Math.Clamp(CloneOffsetUnits + (i * UnitsPerClone), 0, lineLengthUnits - 1);
                float lineLengthPercentage = (float)particlesIndex / LineLengthUnits;

                (bool fullCloneOpacity, float cloneOpacity) = CalculatePlayerCloneOpacity(fillProgress, lineLengthPercentage);

                PlayerClientData playerCloneClientData = playerClones[i];
                PlayerComponent playerClone = playerCloneClientData.PlayerComponent;
                if (!activePassed)
                {
                    if (fullCloneOpacity)
                    {
                        activePassed = true;
                    }

                    if (cloneOpacity > 0.2f)
                    {
                        playerClone.SetCloneFXOpacity(cloneOpacity);
                        if (!playerClone.gameObject.activeSelf)
                        {
                            playerClone.gameObject.SetActive(true);
                        }
                        if (!playerClone.PlayerComponentCloneItems.AnimationStarted)
                        {
                            playerCloneClientData.PlayWalkingState();
                            playerClone.PlayerComponentCloneItems.AnimationStarted = true;
                        }
                    }

                }
                else if (!playerClone.PlayerComponentCloneItems.AnimationTimerCompleted)
                {
                    if (!playerClone.PlayerComponentCloneItems.AnimationTimerSet)
                    {
                        // temp, set based on hardcoded timer instead of motion duration...
                        playerClone.PlayerComponentCloneItems.AnimationTimer.LastCheckedTime = Props.ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                        playerClone.PlayerComponentCloneItems.AnimationTimerSet = true;
                    }
                    else if (playerClone.PlayerComponentCloneItems.AnimationTimer.IsTimeElapsed_FixedUpdateThread())
                    {
                        playerClone.PlayerComponentCloneItems.AnimationTimerCompleted = true;
                        playerClone.Animator.StopPlayback();
                        playerClone.gameObject.SetActive(false);
                    }
                    else
                    {
                        float timerPercentage = playerClone.PlayerComponentCloneItems.AnimationTimer.RemainingDurationPercentage();
                        playerClone.SetCloneFXOpacity(1f - timerPercentage);
                        //Debug.Log($"{i}, {1f - timerPercentage}");
                    }
                }
            }
            
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
    public enum DashParticlesFXTypePrefabPools
    {
        DashParticles,
        ArcPath,
        ElectricTrailRenderer,
        ShockAura,
        CrimsonAuraBlack,
        PortalOrbPurple
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
        PortalOrbPurple
    }
}
