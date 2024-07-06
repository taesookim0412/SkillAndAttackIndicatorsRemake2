using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public class ArcPath_Small_Floating : AbstractAbilityFX
    {
        public float LocalPositionX;
        public float LocalPositionZ;

        public void SetLocalPositionFields(float localPositionX, float localPositionZ)
        {
            LocalPositionX = localPositionX;
            LocalPositionZ = localPositionZ;
        }

    }
}
