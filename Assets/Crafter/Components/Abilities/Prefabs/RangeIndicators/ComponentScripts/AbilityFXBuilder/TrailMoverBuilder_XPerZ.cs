using Assets.Crafter.Components.Editors.ComponentScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class TrailMoverBuilder_XPerZ : AbstractAbilityFXBuilder
    {
        private float[] LocalXPositionsPerZUnit;
        public override void ManualAwake()
        {
            InitializeLocalXPositionsPerZUnit(20);
        }

        private static float[] InitializeLocalXPositionsPerZUnit(int lineLengthUnits)
        {
            // 0, 1, -1, 0, 1, -1
            // i:0, (i % 2) ==0,
            // i:1, (i % 2) == 1,
            // i:2, (i % 2) == 0
            float[] xPositions = new float[lineLengthUnits];

            int numIterations = (int) Math.Floor(lineLengthUnits / 3f);

            for (int i = 0; i < numIterations; i++)
            {
                xPositions[i] = 0f;
                xPositions[i + 1] = 1f;
                xPositions[i + 2] = -1f;
            }

            return xPositions;
        }

        public override void ManualUpdate()
        {
            throw new NotImplementedException();
        }

        public override void EditorDestroy()
        {
            
        }
    }

    [CustomEditor(typeof(TrailMoverBuilder_XPerZ))]
    public class TrailMoverBuilder_XPerZEditor: AbstractEditor<TrailMoverBuilder_XPerZ>
    {
        protected override bool OnInitialize()
        {
            Instance = (TrailMoverBuilder_XPerZ)target;
            return true;
        }
    }
}
