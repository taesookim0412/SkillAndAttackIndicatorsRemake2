using Assets.Crafter.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.DTT.Area_of_Effect_Regions.Demo.Interactive_Demo.Scripts.Observer
{
    public class SkillAndAttackIndicatorSystem : MonoBehaviour
    {
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
        public MonoBehaviour[] AbilityFXTypes;

        [HideInInspector]
        public Camera Camera;

        [HideInInspector]
        private ObserverUpdateProps ObserverUpdateProps;
        [HideInInspector]
        public SkillAndAttackIndicatorObserverProps SkillAndAttackIndicatorObserverProps;
        [HideInInspector]
        public List<SkillAndAttackIndicatorObserver> SkillAndAttackIndicatorObservers = new List<SkillAndAttackIndicatorObserver>();

        [HideInInspector]
        public Dictionary<AbilityProjectorType, Dictionary<AbilityProjectorMaterialType, PoolBagDco<MonoBehaviour>>> ProjectorInstancePools;
        [HideInInspector]
        public Dictionary<AbilityFXType, PoolBagDco<MonoBehaviour>> AbilityFXInstancePools;

        public void Awake()
        {
            Camera = Camera.main;
        }
        public void OnEnable()
        {
            long updateTickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ObserverUpdateProps = new ObserverUpdateProps(updateTickTime);

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

            Dictionary<AbilityFXType, PoolBagDco<MonoBehaviour>> abilityFXInstancePools = new Dictionary<AbilityFXType, PoolBagDco<MonoBehaviour>>(
                    SkillAndAttackIndicatorObserver.AbilityFXTypeNamesLength);
            foreach (MonoBehaviour prefab in AbilityFXTypes)
            {
                if (Enum.TryParse<AbilityFXType>(prefab.name, out AbilityFXType abilityFXType))
                {
                    abilityFXInstancePools[abilityFXType] = new PoolBagDco<MonoBehaviour>(prefab, 30);
                }
            }

            AbilityFXInstancePools = abilityFXInstancePools;
        }
        public void Update()
        {
            ObserverUpdateProps.Update_MainThread();

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
            AbilityFXType abilityFXType)
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
    }
}
