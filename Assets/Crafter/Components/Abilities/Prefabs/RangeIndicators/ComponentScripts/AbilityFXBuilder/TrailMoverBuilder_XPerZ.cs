using Assets.Crafter.Components.Editors.ComponentScripts;
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
        [HideInInspector]
        private float[] LocalXPositionsPerZUnit;
        public override void ManualAwake()
        {
        }

        public void Initialize(ObserverUpdateCache observerUpdateCache, int lineLength)
        {
            InitializeManualAwake();

            LocalXPositionsPerZUnit = InitializeLocalXPositionsPerZUnit(lineLength);
        }

        private static float[] InitializeLocalXPositionsPerZUnit(int lineLength)
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

        public override void ManualUpdate()
        {
            throw new NotImplementedException();
        }

        public override void EditorDestroy()
        {
            
        }
    }

    [CustomEditor(typeof(TrailMoverBuilder_XPerZ))]
    public class TrailMoverBuilder_XPerZEditor : AbstractEditor<TrailMoverBuilder_XPerZ>
    {
        protected override bool OnInitialize(TrailMoverBuilder_XPerZ instance)
        {
            SetObserverUpdateCache();
            instance.Initialize(ObserverUpdateCache, 20);
            return true;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

        }
    }
}
