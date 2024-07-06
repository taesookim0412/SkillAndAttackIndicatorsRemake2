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
        private static readonly int XOffset = Shader.PropertyToID("_XOffset");
        private static readonly int EndYOffsetId = Shader.PropertyToID("_EndYOffset");

        public void SetOffsetFX_FromSky(Random random)
        {
            float xOffset;
            int leftOrRight = random.Next(0, 2);
            
            if (leftOrRight == 0)
            {
                xOffset = random.Next(-75, -24) * 0.01f;
            }
            else
            {
                xOffset = random.Next(25, 76) * 0.01f;
            }
            VisualEffect.SetFloat(XOffset, xOffset);

            //VisualEffect.SetFloat(EndYOffsetId, yOffset);
        }
    }
}
