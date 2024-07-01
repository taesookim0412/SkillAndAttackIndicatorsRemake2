using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;
using Assets.Crafter.Components.Models;
using DTT.AreaOfEffectRegions;

namespace Assets.DTT.Area_of_Effect_Regions.Demo.Interactive_Demo.Scripts.Observer
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

        private static readonly float RadiusHalfDivMult = 1 / 2f;
        // The orthographic length is based on a radius so it is half the desired length.
        // Then, it must be multiplied by half again because it is a "half-length" in the documentation.
        private static readonly float OrthographicRadiusHalfDivMult = 1 / 4f;

        private SkillAndAttackIndicatorObserverProps Props;

        public ObserverStatus ObserverStatus = ObserverStatus.Active;
        
        public readonly AbilityProjectorType AbilityProjectorType;
        public readonly AbilityProjectorMaterialType AbilityProjectorMaterialType;
        public readonly AbilityIndicatorCastType AbilityIndicatorCastType;

        private bool ProjectorSet = false;
        private PoolBagDco<MonoBehaviour> InstancePool;
        private MonoBehaviour ProjectorMonoBehaviour;
        private GameObject ProjectorGameObject;

        private SRPArcRegionProjector ArcRegionProjectorRef;
        private SRPCircleRegionProjector CircleRegionProjectorRef;
        private SRPLineRegionProjector LineRegionProjectorRef;
        private SRPScatterLineRegionProjector ScatterLineRegionProjectorRef;

        private long ChargeDuration;
        private float ChargeDurationSecondsFloat;

        private float PreviousChargeDurationFloatPercentage;

        private long LastTickTime;
        private long ElapsedTime;
        private float ElapsedTimeSecondsFloat;

        public SkillAndAttackIndicatorObserver(AbilityProjectorType abilityProjectorType,
            AbilityProjectorMaterialType abilityProjectorMaterialType, AbilityIndicatorCastType abilityIndicatorCastType,
            SkillAndAttackIndicatorObserverProps skillAndAttackIndicatorObserverProps
            )
        {
            AbilityProjectorType = abilityProjectorType;
            AbilityProjectorMaterialType = abilityProjectorMaterialType;
            AbilityIndicatorCastType = abilityIndicatorCastType;

            Props = skillAndAttackIndicatorObserverProps;
        }

        public void OnUpdate()
        {
            if (!ProjectorSet)
            {
                if (Props.SkillAndAttackIndicatorSystem.ProjectorInstancePools.TryGetValue(AbilityProjectorType, out var abilityMaterialTypesDict) &&
                    abilityMaterialTypesDict.TryGetValue(AbilityProjectorMaterialType, out InstancePool))
                {
                    // 3 texture option indices.
                    ProjectorMonoBehaviour = InstancePool.InstantiatePooled(null);
                    ProjectorGameObject = ProjectorMonoBehaviour.gameObject;

                    switch (AbilityProjectorType)
                    {
                        case AbilityProjectorType.Arc:
                            
                            SRPArcRegionProjector arcRegionProjector = ProjectorGameObject.GetComponent<SRPArcRegionProjector>();
                            arcRegionProjector.Radius = 70;
                            //arcRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);

                            arcRegionProjector.GenerateProjector();

                            ArcRegionProjectorRef = arcRegionProjector;
                            break;
                        case AbilityProjectorType.Circle:
                            SRPCircleRegionProjector circleRegionProjector = ProjectorGameObject.GetComponent<SRPCircleRegionProjector>();
                            circleRegionProjector.Radius = 70;

                            //circleRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);
                            circleRegionProjector.GenerateProjector();

                            CircleRegionProjectorRef = circleRegionProjector;
                            break;
                        case AbilityProjectorType.Line:
                            SRPLineRegionProjector lineRegionProjector = ProjectorGameObject.GetComponent<SRPLineRegionProjector>();

                            // hard coded length units.
                            int lineLengthUnits = 25;
                            // multiply it by the orthographicRadiusHalfDivMultiplier
                            //float orthographicLength = lineLengthUnits * OrthographicRadiusHalfDivMult;
                            lineRegionProjector.Length = lineLengthUnits * RadiusHalfDivMult;

                            //lineRegionProjector.SetIgnoreLayers(Props.SkillAndAttackIndicatorSystem.ProjectorIgnoreLayersMask);
                            lineRegionProjector.GenerateProjector();
                            lineRegionProjector.SetTerrainLayer(Props.SkillAndAttackIndicatorSystem.TerrainRenderingLayer);
                            lineRegionProjector.Depth = 100f;

                            LineRegionProjectorRef = lineRegionProjector;
                            break;
                        case AbilityProjectorType.ScatterLine:
                            SRPScatterLineRegionProjector scatterLineRegionProjector = ProjectorGameObject.GetComponent<SRPScatterLineRegionProjector>();
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

                    switch (AbilityProjectorMaterialType) {
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
                    LastTickTime = Props.ObserverUpdateProps.UpdateTickTime;
                }
                else
                {
                    ObserverStatus = ObserverStatus.Remove;
                    return;
                }
            }
            else
            {
                long elapsedTickTime = Props.ObserverUpdateProps.UpdateTickTime - LastTickTime;
                LastTickTime = Props.ObserverUpdateProps.UpdateTickTime;
                ElapsedTime += elapsedTickTime;
                ElapsedTimeSecondsFloat += elapsedTickTime * 0.001f;

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
                                float newFillProgress = EaseInOutExpo(chargeDurationPercentage);
                                LineRegionProjectorRef.FillProgress = newFillProgress;
                                LineRegionProjectorRef.UpdateProjectors();
                                PreviousChargeDurationFloatPercentage = chargeDurationPercentage;
                            }
                        }
                        break;

                    case AbilityProjectorType.ScatterLine:

                        break;

                }
            }

            Vector3 terrainPosition = GetTerrainPosition();
            ProjectorGameObject.transform.position = terrainPosition;

            if (ElapsedTime > ChargeDuration)
            {
                InstancePool.ReturnPooled(ProjectorMonoBehaviour);
                ObserverStatus = ObserverStatus.Remove;
            }
        }
        private float EaseInOutExpo(float percentage)
        {
            return percentage < 0.5f ? (float)Math.Pow(2, 20 * percentage - 10) / 2 :
                (2 - (float)Math.Pow(2, -20 * percentage + 10)) / 2;
        }
        private Vector3 GetTerrainPosition()
        {
            //if (EventSystem.current.IsPointerOverGameObject() || !_isMouseOverGameWindow || IsPointerOverUIElement(GetEventSystemRaycastResults()))
            //    return _anchorPoint.transform.position;
            Ray ray = Props.SkillAndAttackIndicatorSystem.Camera.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Props.SkillAndAttackIndicatorSystem.TerrainLayer) ? 
                hit.point + new Vector3(0, 50, 0) 
                : new Vector3(0, 50, 0);
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
}
