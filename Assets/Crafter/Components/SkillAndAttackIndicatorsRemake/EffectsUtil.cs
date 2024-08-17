using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public static class EffectsUtil
    {

        public static float EaseInOutQuad(float percentage)
        {
            return percentage * percentage;
        }

        public static long[] GenerateTimeRequiredForDistancesPerModifiedUnit(int lineLengthUnits, long chargeDuration, int zUnitsPerIndex)
        {
            // iterate the distances
            // find the fillPercentage
            // invert the fillPercentage into time
            int modifiedLineLengthUnits = PartialDataTypesUtil.Round((float) lineLengthUnits / (float) zUnitsPerIndex);

            // use PlusOne because index 0 is a buffer at position z=0.
            int modifiedLineLenghtUnitBuffered = modifiedLineLengthUnits + 1;

            float lineLengthUnitsFloat = (float)lineLengthUnits;
            float modifiedLineLengthUnitsFloat = (float)modifiedLineLengthUnits;

            float chargeDurationFloat = (float)chargeDuration;

            int startDistUnitsIndex = 1;
            long prevChargeDurationIterationTimeRequired = 0L;

            long[] timeRequiredForDistancesPerModifiedUnit = new long[modifiedLineLenghtUnitBuffered];
            for (int i = 1; i < lineLengthUnits + 1; i++)
            {
                // i = 10, % = 0.33
                float lineLengthPercentage = i / lineLengthUnitsFloat;
                // i = 10, fp% = ~0.1089
                float fillProgress = EaseInOutQuad(lineLengthPercentage);
                // i = 10, zDist = 0.11 * 30 = 3.3
                int zDistanceUnitsIndex = (int)(fillProgress * modifiedLineLengthUnits);
                long timeValue = (long)(lineLengthPercentage * chargeDurationFloat);
                // i = 10, fill prevIdx to 3.3 (or zDistanceUnits) with timePercentage.

                int interpLen = zDistanceUnitsIndex - startDistUnitsIndex;
                if (interpLen < 1)
                {
                    // interpLen could be negative since startDistUnitsIndex starts at 1.
                    continue;
                }
                float interpLenFloat = (float)interpLen;
                long timeValueDifference = timeValue - prevChargeDurationIterationTimeRequired;
                for (int j = 0; j < interpLen; j++)
                {
                    long interpTimeValue = (long)(prevChargeDurationIterationTimeRequired + timeValueDifference * ((j + 1) / interpLenFloat));
                    int interpIndex = startDistUnitsIndex + j;
                    if (interpIndex < modifiedLineLengthUnits)
                    {
                        timeRequiredForDistancesPerModifiedUnit[interpIndex] = interpTimeValue;
                    }
                    else
                    {
                        // fp error...? let it fill old values...
                        Debug.LogError("FP error when interp-ing distance times...");
                        break;
                    }
                    if (j == interpLen - 1)
                    {
                        prevChargeDurationIterationTimeRequired = interpTimeValue;
                        startDistUnitsIndex += interpLen;
                    }
                }

            }
            // fill the remaining values...
            if (startDistUnitsIndex < modifiedLineLenghtUnitBuffered)
            {
                long lastValue = startDistUnitsIndex > 0 ? timeRequiredForDistancesPerModifiedUnit[startDistUnitsIndex - 1] : 0L;
                long timeValueDifference = chargeDuration - lastValue;

                int interpLen = modifiedLineLenghtUnitBuffered - startDistUnitsIndex;
                float interpLenFloat = (float)interpLen;
                for (int j = 0; j < interpLen; j++)
                {
                    long interpTimeValue = (long)(prevChargeDurationIterationTimeRequired + timeValueDifference * ((j + 1) / interpLenFloat));
                    int interpIndex = startDistUnitsIndex + j;
                    if (interpIndex < modifiedLineLenghtUnitBuffered)
                    {
                        timeRequiredForDistancesPerModifiedUnit[interpIndex] = interpTimeValue;
                    }
                }
            }

            return timeRequiredForDistancesPerModifiedUnit;
        }
    }
}
