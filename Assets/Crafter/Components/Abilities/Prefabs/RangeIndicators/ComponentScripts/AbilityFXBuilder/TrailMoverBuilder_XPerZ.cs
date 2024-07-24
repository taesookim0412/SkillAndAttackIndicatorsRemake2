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
        public Vector3[] WorldPositionsPerZUnit;
        [HideInInspector]
        public int PositionIndex;
        
        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache,
            ElectricTrail electricTrail,
            int lineLength,
            SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX, float startPositionZ, float yRotation)
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
                startPositionX, startPositionZ, yRotation);

            PositionIndex = 0;
        }
        protected Vector3[] InitializeWorldPositionsPerZUnit(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem,
            float startPositionX,
            float startPositionZ,
            float yRotation)
        {
            float[] localXPositionsPerZUnit = LocalXPositionsPerZUnit;

            float cosYAngle = (float)Math.Cos(yRotation * Mathf.Deg2Rad);
            float sinYAngle = (float)Math.Sin(yRotation * Mathf.Deg2Rad);

            Vector3[] worldPositions = new Vector3[localXPositionsPerZUnit.Length];

            for (int i = 0; i < localXPositionsPerZUnit.Length; i++)
            {
                float localPositionX = localXPositionsPerZUnit[i];

                float rotatedLocalPositionX = i * sinYAngle + localPositionX * cosYAngle;
                float rotatedLocalPositionZ = i * cosYAngle - localPositionX * sinYAngle;

                float worldPositionX = startPositionX + rotatedLocalPositionX;
                float worldPositionZ = startPositionZ + rotatedLocalPositionZ;

                worldPositions[i] = new Vector3(worldPositionX, skillAndAttackIndicatorSystem.GetTerrainHeight(worldPositionX, worldPositionZ) + TrailRendererYOffset, worldPositionZ);
            }

            return worldPositions;
        }

        private float[] InitializeLocalXPositionsPerZUnit(int lineLength)
        {
            float[] xPositions = new float[lineLength];

            int numIterations = (int) Math.Floor(lineLength / 3f);

            for (int i = 0; i < lineLength; i++)
            {
                int xPos = i % 3;

                if (xPos > 1)
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
            if (zUnitsIndex < LineLength)
            {
                int positionIndex = PositionIndex;
                if (positionIndex < LineLength)
                {
                    if (positionIndex < zUnitsIndex)
                    {
                        transform.position = WorldPositionsPerZUnit[positionIndex];

                        PositionIndex = zUnitsIndex;
                    }
                    else
                    {
                        //TODO: Interp between PositionIndex and next zUnitsIndex with timeRequiredForDistances and Time.fixedDeltaTime.
                        
                    }
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
                    instance.Initialize(ObserverUpdateCache, electricTrail, 20, system,
                        position.x, position.z, 0f);
                    TryAddParticleSystem(instance.gameObject);
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
            float fillProgress = (ObserverUpdateCache.UpdateTickTimeFixedUpdate % 5000L) / 5000f;
            Instance.ManualUpdate(fillProgress);
            if (Instance.PositionIndex == Instance.LineLength)
            {
                Instance.transform.position = Instance.WorldPositionsPerZUnit[0];
            }
        }

        protected override void EditorDestroy()
        {
            GameObject.DestroyImmediate(Instance.ElectricTrail.gameObject);
            Instance.ElectricTrail = null;
        }
    }
}
