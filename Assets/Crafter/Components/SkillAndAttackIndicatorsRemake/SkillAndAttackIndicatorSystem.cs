using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFX;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.Projectors;
using Assets.Crafter.Components.Constants;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Models.dpo.TrailEffectsDpo;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.Systems.Observers;
using Cinemachine;
using DTT.AreaOfEffectRegions;
using StarterAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class SkillAndAttackIndicatorSystem : MonoBehaviour
    {

        // these are static due to cross-assembly when migrated.
        public const long FixedTimestep = 20L;
        public const float FixedTrailTimestepSec = 0.01f;
        public const float FixedTrailTimestepSecReciprocal = 1f / FixedTrailTimestepSec;
        public const float ONE_THIRD = 1 / 3f;
        public const float TWO_THIRDS = 2 / 3f;


        [SerializeField]
        public LayerMask TerrainLayer;
        [SerializeField]
        public Terrain Terrain;
        [NonSerialized, HideInInspector]
        private bool TerrainValuesCached = false;
        [NonSerialized, HideInInspector]
        private Vector3 TerrainStart = Vector3.zero;
        [NonSerialized, HideInInspector]
        private Vector3 TerrainSize = Vector3.one;
        [SerializeField]
        public AbstractProjector[] Projectors;
        [SerializeField]
        public Material[] LineProjectorMaterials;
        [SerializeField]
        public AbstractAbilityFX[] AbilityFXComponentPrefabs;
        [SerializeField]
        public PlayerComponent PlayerComponent;
        [SerializeField]
        public CinemachineVirtualCamera PlayerFollowCamera;
        [SerializeField]
        public CinemachineVirtualCamera PlayerMoverCamera;

        [NonSerialized, HideInInspector]
        public PlayerClientData PlayerClientData;
        [NonSerialized, HideInInspector]
        public Camera Camera;

        [NonSerialized, HideInInspector]
        private ObserverUpdateProps ObserverUpdateProps;
        [NonSerialized, HideInInspector]
        private ObserverUpdateCache ObserverUpdateCache;
        [NonSerialized, HideInInspector]
        public SkillAndAttackIndicatorObserverProps SkillAndAttackIndicatorObserverProps;
        [NonSerialized, HideInInspector]
        public List<SkillAndAttackIndicatorObserver> SkillAndAttackIndicatorObservers = new List<SkillAndAttackIndicatorObserver>();
        [NonSerialized, HideInInspector]
        public DashAbilityTriggerObserverProps DashAbilityTriggerObserverProps;
        [NonSerialized, HideInInspector]
        public List<DashAbilityTriggerObserver<DashAbilityTriggerObserverProps>> DashAbilityTriggerObservers = new List<DashAbilityTriggerObserver<DashAbilityTriggerObserverProps>>();

        [NonSerialized, HideInInspector]
        public Dictionary<AbilityProjectorType, Dictionary<AbilityProjectorMaterialType, PoolBagDco<AbstractProjector>>> ProjectorInstancePools;
        [NonSerialized, HideInInspector]
        public Dictionary<AbilityIndicatorFXType, PoolBagDco<AbstractAbilityFX>[]> AbilityIndicatorFXInstancePools;
        [NonSerialized, HideInInspector]
        public Dictionary<AbilityTriggerFXType, PoolBagDco<AbstractAbilityFX>[]> AbilityTriggerFXInstancePools;
        [NonSerialized, HideInInspector]
        public Dictionary<Guid, PoolBagDco<PlayerComponent>> PlayerCloneInstancePools;

        [NonSerialized, HideInInspector]
        public Guid PlayerGuid = Guid.NewGuid();
        public void Awake()
        {
            PlayerClientData = new PlayerClientData(PlayerGuid, PlayerComponent);
            Camera = Camera.main;
            // Might have to reset virtual camera other times too.
            ResetVirtualCamera();
        }
        public void OnEnable()
        {
            long updateTickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ObserverUpdateCache = new ObserverUpdateCache(updateTickTime);
            ObserverUpdateProps = new ObserverUpdateProps(ObserverUpdateCache);

            SkillAndAttackIndicatorObserverProps = new SkillAndAttackIndicatorObserverProps(this, ObserverUpdateCache);

            SkillAndAttackIndicatorObservers = new List<SkillAndAttackIndicatorObserver>();

            DashAbilityTriggerObserverProps = new DashAbilityTriggerObserverProps(this, ObserverUpdateProps);

            DashAbilityTriggerObservers = new List<DashAbilityTriggerObserver<DashAbilityTriggerObserverProps>>();

            Dictionary<AbilityProjectorType, Dictionary<AbilityProjectorMaterialType, PoolBagDco<AbstractProjector>>> projectorInstancePools = new
                Dictionary<AbilityProjectorType, Dictionary<AbilityProjectorMaterialType, PoolBagDco<AbstractProjector>>>(
                    SkillAndAttackIndicatorObserver.AbilityProjectorTypeNamesLength);

            string lineProjectorTypeString = AbilityProjectorType.LineProjector.ToString();

            MonoBehaviour lineRegionProjectorPrefab = Projectors.FirstOrDefault(prefab => prefab.name == lineProjectorTypeString);
            Dictionary<AbilityProjectorMaterialType, PoolBagDco<AbstractProjector>> lineMaterialsDict = new Dictionary<AbilityProjectorMaterialType, PoolBagDco<AbstractProjector>>(LineProjectorMaterials.Length);
            foreach (Material lineProjectorMaterial in LineProjectorMaterials)
            {
                AbilityProjectorMaterialType abilityProjectorMaterialType = Enum.Parse<AbilityProjectorMaterialType>(lineProjectorMaterial.name);

                LineProjector lineProjectorInstance = (LineProjector) GameObject.Instantiate(lineRegionProjectorPrefab, null);
                lineProjectorInstance.Projector.material = lineProjectorMaterial;
                lineProjectorInstance.gameObject.SetActive(false);

                lineMaterialsDict[abilityProjectorMaterialType] = new PoolBagDco<AbstractProjector>(lineProjectorInstance, 10);
            }
            projectorInstancePools[AbilityProjectorType.LineProjector] = lineMaterialsDict;

            ProjectorInstancePools = projectorInstancePools;

            Dictionary<AbilityFXComponentType, AbstractAbilityFX> abilityFXComponentTypeDict = new Dictionary<AbilityFXComponentType, AbstractAbilityFX>(
                SkillAndAttackIndicatorObserver.AbilityFXComponentTypeNamesLength);

            foreach (AbstractAbilityFX prefab in AbilityFXComponentPrefabs)
            {
                if (Enum.TryParse(prefab.name, out AbilityFXComponentType abilityFXComponentType))
                {
                    abilityFXComponentTypeDict[abilityFXComponentType] = prefab;
                }
            }

            Dictionary<AbilityIndicatorFXType, PoolBagDco<AbstractAbilityFX>[]> abilityIndicatorFXInstancePools = new Dictionary<AbilityIndicatorFXType, PoolBagDco<AbstractAbilityFX>[]>(
                    SkillAndAttackIndicatorObserver.AbilityFXTypeNamesLength);

            abilityIndicatorFXInstancePools[AbilityIndicatorFXType.DashBlinkAbility] = new PoolBagDco<AbstractAbilityFX>[0];

            AbilityIndicatorFXInstancePools = abilityIndicatorFXInstancePools;

            AbilityTriggerFXInstancePools = InitializeAbilityTriggerFXInstancePools(abilityFXComponentTypeDict);

            PlayerComponent playerComponentTransparentCloneInstance = PlayerComponent.CreateInactiveTransparentCloneInstance();

            Dictionary<Guid, PoolBagDco<PlayerComponent>> playerCloneInstancePools = new Dictionary<Guid, PoolBagDco<PlayerComponent>>(1)
            {
                { PlayerGuid, new PoolBagDco<PlayerComponent>(playerComponentTransparentCloneInstance, 10) }
            };

            PlayerCloneInstancePools = playerCloneInstancePools;
        }

        private Dictionary<AbilityTriggerFXType, PoolBagDco<AbstractAbilityFX>[]> InitializeAbilityTriggerFXInstancePools(Dictionary<AbilityFXComponentType, AbstractAbilityFX> abilityFXComponentTypeDict)
        {
            Dictionary<AbilityTriggerFXType, PoolBagDco<AbstractAbilityFX>[]> abilityTriggerFXInstancePools = new Dictionary<AbilityTriggerFXType,
    PoolBagDco<AbstractAbilityFX>[]>(AbilityFXDefinition.AbilityTriggerFXTypeEnumLength);
            if (abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.PlayerBlinkBuilder_Source, out AbstractAbilityFX playerBlinkBuilderSourcePrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.PlayerBlinkBuilder_Dest, out AbstractAbilityFX playerBlinkBuilderDestPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.DashBlinkAbilityChain, out AbstractAbilityFX dashBlinkAbilityChainPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.BlinkRibbonTrailRenderer, out AbstractAbilityFX blinkRibbonTrailRendererPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.TrailMoverBuilder_TargetPos, out AbstractAbilityFX trailMoverBuilderTargetPosPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.CameraMoverBuilder, out AbstractAbilityFX cameraMoverBuilderPrefab))
            {
                PoolBagDco<AbstractAbilityFX> blinkRibbonTrailRendererBag = new PoolBagDco<AbstractAbilityFX>(blinkRibbonTrailRendererPrefab, 30);

                PoolBagDco<AbstractAbilityFX>[] dashTriggerPoolBag = new PoolBagDco<AbstractAbilityFX>[6];
                dashTriggerPoolBag[(int)DashAbilityTriggerTypeInstancePools.PlayerBlinkBuilder_Source] = new PoolBagDco<AbstractAbilityFX>(playerBlinkBuilderSourcePrefab, 30);
                dashTriggerPoolBag[(int)DashAbilityTriggerTypeInstancePools.PlayerBlinkBuilder_Dest] = new PoolBagDco<AbstractAbilityFX>(playerBlinkBuilderDestPrefab, 30);
                dashTriggerPoolBag[(int)DashAbilityTriggerTypeInstancePools.DashBlinkAbilityChain] = new PoolBagDco<AbstractAbilityFX>(dashBlinkAbilityChainPrefab, 30);
                dashTriggerPoolBag[(int)DashAbilityTriggerTypeInstancePools.BlinkRibbonTrailRenderer] = new PoolBagDco<AbstractAbilityFX>(blinkRibbonTrailRendererPrefab, 30);
                dashTriggerPoolBag[(int)DashAbilityTriggerTypeInstancePools.TrailMoverBuilder_TargetPos] = new PoolBagDco<AbstractAbilityFX>(trailMoverBuilderTargetPosPrefab, 30);
                dashTriggerPoolBag[(int)DashAbilityTriggerTypeInstancePools.CameraMoverBuilder] = new PoolBagDco<AbstractAbilityFX>(cameraMoverBuilderPrefab, 30);

                abilityTriggerFXInstancePools[AbilityTriggerFXType.DashBlinkTrigger] = dashTriggerPoolBag;
            }

            return abilityTriggerFXInstancePools;
        }
        public void Update()
        {
            ObserverUpdateCache.Update_RenderThread();

            //TODO: Migrate to AbstractUpdateObserver.
            if (SkillAndAttackIndicatorObservers.Count > 0)
            {
                bool foundRemove = false;
                for (int i = 0; i < SkillAndAttackIndicatorObservers.Count; i++)
                {
                    SkillAndAttackIndicatorObservers[i].OnUpdate();
                    if (!foundRemove && SkillAndAttackIndicatorObservers[i].ObserverStatus == ObserverStatus.Remove)
                    {
                        foundRemove = true;
                    }
                }
                // lazy way of removing for remake prototype
                if (foundRemove)
                {
                    SkillAndAttackIndicatorObservers = SkillAndAttackIndicatorObservers.Where(observer => observer.ObserverStatus != ObserverStatus.Remove).ToList();
                }
            }

            DashAbilityTriggerObservers.UpdateObservers<DashAbilityTriggerObserver<DashAbilityTriggerObserverProps>, DashAbilityTriggerObserverProps>();

        }

        public void SetPlayerMoverCamera(Transform follow, Transform lookAt)
        {
            PlayerMoverCamera.m_Follow = follow;
            PlayerMoverCamera.m_LookAt = lookAt;
            PlayerMoverCamera.Priority = 10;
            PlayerFollowCamera.Priority = 9;
        }
        public void ResetVirtualCamera()
        {
            PlayerMoverCamera.m_Follow = null;
            PlayerMoverCamera.m_LookAt = null;
            PlayerMoverCamera.Priority = 9;
            PlayerFollowCamera.Priority = 10;

        }
        /// Copyright
        /// Note: Now it's slightly off from the position map.
        /// <summary>
        /// This assumes worldX and worldZ are clamped between local terrain axis 0 and axis.size - 1.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldZ"></param>
        /// <returns></returns>
        public float GetTerrainHeight(float worldX, float worldZ)
        {
            if (!TerrainValuesCached)
            {
                TerrainStart = Terrain.GetPosition();
                TerrainSize = Terrain.terrainData.size;
                TerrainValuesCached = true;
            }

            return TerrainStart.y + Terrain.terrainData.GetInterpolatedHeight((worldX - TerrainStart.x) / TerrainSize.x,
                (worldZ - TerrainStart.z) / TerrainSize.z);
        }
        /// Copyright
        /// Note: It's wrong.
        /// <summary>
        /// This assumes worldX and worldZ are clamped between local terrain axis 0 and axis.size - 1.
        /// </summary>
        /// <param name="worldX"></param>
        /// <param name="worldZ"></param>
        /// <returns></returns>
        //public float GetTerrainHeight(float worldX, float worldZ)
        //{
        //    if (!TerrainValuesCached)
        //    {
        //        TerrainStart = Terrain.GetPosition();
        //        TerrainSize = Terrain.terrainData.size;
        //        TerrainValuesCached = true;
        //    }
        //    float terrainSizeZ = TerrainSize.z;

        //    // add 1f to match accurate position map.
        //    float correctedInterpolatedHeightLocalZ = Math.Clamp(worldZ - TerrainStart.z + 1f, 0f, terrainSizeZ);

        //    return TerrainStart.y + Terrain.terrainData.GetInterpolatedHeight((worldX - TerrainStart.x) / TerrainSize.x,
        //        correctedInterpolatedHeightLocalZ / terrainSizeZ);
        //}
        public void TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType abilityProjectorType,
            AbilityProjectorMaterialType abilityProjectorMaterialType,
            AbilityIndicatorCastType abilityIndicatorCastType,
            AbilityIndicatorFXType abilityFXType)
        {
            bool attemptTriggerUpdate;
            switch (abilityIndicatorCastType)
            {
                case AbilityIndicatorCastType.DoubleCast:
                    attemptTriggerUpdate = true;
                    break;
                default:
                    attemptTriggerUpdate = false;
                    break;
            }

            bool addPending = true;
            if (attemptTriggerUpdate)
            {
                for (int i = 0; i < SkillAndAttackIndicatorObservers.Count; i++)
                {
                    if (SkillAndAttackIndicatorObservers[i].AbilityProjectorType == abilityProjectorType &&
                        SkillAndAttackIndicatorObservers[i].AbilityIndicatorCastType == AbilityIndicatorCastType.DoubleCast)
                    {
                        SkillAndAttackIndicatorObservers[i].TriggerDoubleCast();
                        addPending = false;
                        break;
                    }
                }
            }
            if (addPending)
            {
                SkillAndAttackIndicatorObserver skillAndAttackIndicatorObserver = new SkillAndAttackIndicatorObserver(abilityProjectorType,
                    abilityProjectorMaterialType,
                    abilityIndicatorCastType,
                    abilityFXType,
                    SkillAndAttackIndicatorObserverProps);

                SkillAndAttackIndicatorObservers.Add(skillAndAttackIndicatorObserver);
            }
        }

        public void AddDashAbilityTriggerObserver(Vector3 targetPosition)
        {
            DashAbilityTriggerObserver<DashAbilityTriggerObserverProps> dashAbilityTriggerObserver = new DashAbilityTriggerObserver<DashAbilityTriggerObserverProps>(
                PlayerClientData,
                targetPosition, DashAbilityTriggerObserverProps);
            DashAbilityTriggerObservers.Add(dashAbilityTriggerObserver);
        }
        public static bool IsValueOvershot(int direction, float maxValue, float currentValue)
        {
            if (direction > 0)
            {
                return currentValue > maxValue + PartialMathUtil.FLOAT_TOLERANCE;
            }
            else if (direction < 0)
            {
                return currentValue < maxValue - PartialMathUtil.FLOAT_TOLERANCE;
            }
            else
            {
                return false;
            }
        }
    }
}
