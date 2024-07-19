using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.Systems.Observers;
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
        public static readonly long FixedTimestep = 20L;
        public static readonly float ONE_THIRD = 1 / 3f;
        public static readonly float TWO_THIRDS = 2 / 3f;


        [SerializeField]
        public LayerMask TerrainLayer;
        [SerializeField]
        public int TerrainRenderingLayer;
        [SerializeField]
        public Terrain Terrain;
        [HideInInspector]
        private bool TerrainValuesCached = false;
        [HideInInspector]
        private Vector3 TerrainStart = Vector3.zero;
        [HideInInspector]
        private Vector3 TerrainSize = Vector3.one;
        [SerializeField]
        public MonoBehaviour[] Projectors;
        [SerializeField]
        public AbstractAbilityFX[] AbilityFXComponentPrefabs;
        [SerializeField]
        public PlayerComponent PlayerComponent;

        [HideInInspector]
        public PlayerClientData PlayerClientData;
        [HideInInspector]
        public Camera Camera;

        [HideInInspector]
        private ObserverUpdateCache ObserverUpdateProps;
        [HideInInspector]
        public SkillAndAttackIndicatorObserverProps SkillAndAttackIndicatorObserverProps;
        [HideInInspector]
        public List<SkillAndAttackIndicatorObserver> SkillAndAttackIndicatorObservers = new List<SkillAndAttackIndicatorObserver>();

        [HideInInspector]
        public Dictionary<AbilityProjectorType, Dictionary<AbilityProjectorMaterialType, PoolBagDco<MonoBehaviour>>> ProjectorInstancePools;
        [HideInInspector]
        public Dictionary<AbilityIndicatorFXType, PoolBagDco<AbstractAbilityFX>[]> AbilityIndicatorFXInstancePools;
        [HideInInspector]
        public Dictionary<AbilityTriggerFXType, PoolBagDco<AbstractAbilityFX>[]> AbilityTriggerFXInstancePools;
        [HideInInspector]
        public Dictionary<Guid, PoolBagDco<PlayerComponent>> PlayerCloneInstancePools;

        [HideInInspector]
        public Guid PlayerGuid = Guid.NewGuid();
        public void Awake()
        {
            PlayerClientData = new PlayerClientData(PlayerComponent);
            Camera = Camera.main;
        }
        public void OnEnable()
        {
            long updateTickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ObserverUpdateProps = new ObserverUpdateCache(updateTickTime);

            SkillAndAttackIndicatorObserverProps = new SkillAndAttackIndicatorObserverProps(this, ObserverUpdateProps);

            SkillAndAttackIndicatorObservers = new List<SkillAndAttackIndicatorObserver>();

            Dictionary<AbilityProjectorType, Dictionary<AbilityProjectorMaterialType, PoolBagDco<MonoBehaviour>>> projectorInstancePools = new
                Dictionary<AbilityProjectorType, Dictionary<AbilityProjectorMaterialType, PoolBagDco<MonoBehaviour>>>(
                    SkillAndAttackIndicatorObserver.AbilityProjectorTypeNamesLength);

            int projectorIndex = 0;
            foreach (string projectorTypeNameString in SkillAndAttackIndicatorObserver.AbilityProjectorTypeNames)
            {
                AbilityProjectorType abilityProjectorType = Enum.Parse<AbilityProjectorType>(projectorTypeNameString);

                Dictionary<AbilityProjectorMaterialType, PoolBagDco<MonoBehaviour>> projectorTypeDict = new
                    Dictionary<AbilityProjectorMaterialType, PoolBagDco<MonoBehaviour>>(SkillAndAttackIndicatorObserver.AbilityProjectorMaterialTypeNamesLength);
                foreach (string projectTypeMaterialString in SkillAndAttackIndicatorObserver.AbilityProjectorMaterialTypeNames)
                {
                    AbilityProjectorMaterialType materialType = Enum.Parse<AbilityProjectorMaterialType>(projectTypeMaterialString);

                    projectorTypeDict[materialType] = new PoolBagDco<MonoBehaviour>(Projectors[projectorIndex++], 30);
                }

                projectorInstancePools[abilityProjectorType] = projectorTypeDict;
            }

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

            if (abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.DashParticles, out AbstractAbilityFX dashParticlesPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.ArcPath, out AbstractAbilityFX arcPathPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.ElectricTrailRenderer, out AbstractAbilityFX electricTrailRendererPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.ShockAura, out AbstractAbilityFX shockAuraPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.CrimsonAuraBlack, out AbstractAbilityFX crimsonAuraDarkPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.PortalOrbPurple, out AbstractAbilityFX portalOrbPurplePrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.PortalBuilder_Source, out AbstractAbilityFX portalBuilderSrcPrefab) &&
                abilityFXComponentTypeDict.TryGetValue(AbilityFXComponentType.PortalBuilder_Dest, out AbstractAbilityFX portalBuilderDestPrefab)
                )
            {
                PoolBagDco<AbstractAbilityFX>[] dashParticlesPoolBag = new PoolBagDco<AbstractAbilityFX>[8];
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.DashParticles] = new PoolBagDco<AbstractAbilityFX>(dashParticlesPrefab, 30);
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.ArcPath] = new PoolBagDco<AbstractAbilityFX>(arcPathPrefab, 30);
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.ElectricTrailRenderer] = new PoolBagDco<AbstractAbilityFX>(electricTrailRendererPrefab, 30);
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.ShockAura] = new PoolBagDco<AbstractAbilityFX>(shockAuraPrefab, 30);
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.CrimsonAuraBlack] = new PoolBagDco<AbstractAbilityFX>(crimsonAuraDarkPrefab, 30);
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.PortalOrbPurple] = new PoolBagDco<AbstractAbilityFX>(portalOrbPurplePrefab, 30);
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.PortalBuilder_Source] = new PoolBagDco<AbstractAbilityFX>(portalBuilderSrcPrefab, 30);
                dashParticlesPoolBag[(int)DashParticlesFXTypeInstancePools.PortalBuilder_Dest] = new PoolBagDco<AbstractAbilityFX>(portalBuilderDestPrefab, 30);

                abilityIndicatorFXInstancePools[AbilityIndicatorFXType.DashParticles] = dashParticlesPoolBag;
            }

            AbilityIndicatorFXInstancePools = abilityIndicatorFXInstancePools;

            PlayerComponent playerComponentTransparentCloneInstance = PlayerComponent.CreateInactiveTransparentCloneInstance();

            Dictionary<Guid, PoolBagDco<PlayerComponent>> playerCloneInstancePools = new Dictionary<Guid, PoolBagDco<PlayerComponent>>(1)
            {
                { PlayerGuid, new PoolBagDco<PlayerComponent>(playerComponentTransparentCloneInstance, 10) }
            };

            PlayerCloneInstancePools = playerCloneInstancePools;
        }
        public void FixedUpdate()
        {
            ObserverUpdateProps.Update_FixedUpdate();

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
        }
        /// Copyright
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
            float terrainSizeZ = TerrainSize.z;

            // add 1f to match accurate position map.
            float correctedInterpolatedHeightLocalZ = Math.Clamp(worldZ - TerrainStart.z + 1f, 0f, terrainSizeZ);

            return TerrainStart.y + Terrain.terrainData.GetInterpolatedHeight((worldX - TerrainStart.x) / TerrainSize.x,
                correctedInterpolatedHeightLocalZ / terrainSizeZ);
        }
        public void TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType abilityProjectorType,
            AbilityProjectorMaterialType abilityProjectorMaterialType,
            AbilityIndicatorCastType abilityIndicatorCastType,
            AbilityIndicatorFXType[] abilityFXTypes)
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
                    abilityFXTypes,
                    SkillAndAttackIndicatorObserverProps);

                SkillAndAttackIndicatorObservers.Add(skillAndAttackIndicatorObserver);
            }
        }
    }
}
