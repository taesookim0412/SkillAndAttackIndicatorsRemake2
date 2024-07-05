using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public class ArcPath : AbstractAbilityFX
    {
        private static readonly int DiagXOffsetId = Shader.PropertyToID("_DiagXOffset");
        private static readonly int EndYOffsetId = Shader.PropertyToID("_EndYOffset");

        public void SetOffsetFX(Random random)
        {
            float xOffset = random.Next(-4, 5);
            float yOffset = random.Next(2, 7);

            VisualEffect.SetFloat(DiagXOffsetId, xOffset);
            VisualEffect.SetFloat(EndYOffsetId, yOffset);
        }
    }
}
