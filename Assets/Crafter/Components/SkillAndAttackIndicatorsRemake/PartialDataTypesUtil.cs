using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public static class PartialDataTypesUtil
    {
        public static int Round(float num)
        {
            if (num < 0)
            {
                return (int)(num - 0.5f);
            }
            return (int)(num + 0.5f);
        }
    }
}
