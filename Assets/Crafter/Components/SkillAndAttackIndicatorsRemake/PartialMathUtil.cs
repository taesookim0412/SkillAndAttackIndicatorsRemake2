﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public static class PartialMathUtil
    {
        public const float FLOAT_TOLERANCE = 0.0001f;
        public const float FLOAT_TOLERANCE_NEGATIVE = -0.0001f;
        public const float PI = 3.14159274F;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 57.29578f;
        public static float RepeatRotation(float t)
        {
            return Clamp(t - (float)Math.Floor(t / 360f) * 360f, 0f, 360f);
        }
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }

    }
}
