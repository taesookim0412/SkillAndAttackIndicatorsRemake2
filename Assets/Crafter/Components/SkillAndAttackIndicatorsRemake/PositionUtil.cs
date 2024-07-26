using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public static class PositionUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxValue"></param>
        /// <param name="positiveMultiple">Must be positive, dt is always positive.</param>
        /// <returns></returns>
        public static float CalculateClosestMultipleOrClamp(float value, float maxValue, float positiveMultiple)
        {
            if (positiveMultiple < SkillAndAttackIndicatorSystem.FLOAT_TOLERANCE)
            {
                return value;
            }
            float difference = maxValue - value;

            if (positiveMultiple > (float) Math.Abs(difference))
            {
                return maxValue;
            }

            float quotient = difference / positiveMultiple;
            float remainder = quotient - (int)quotient;
            // if lt 10% of the multiple then just return the maxvalue
            if (remainder > -0.1f && remainder < 0.1f)
            {
                return maxValue;
            }
            else
            {
                return maxValue - (remainder * positiveMultiple);
            }

        }
    }
}
