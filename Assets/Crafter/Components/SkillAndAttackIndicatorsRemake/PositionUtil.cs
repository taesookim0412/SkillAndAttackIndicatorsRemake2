using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
        public static float CalculateClosestMultipleOrClamp(float value, float maxValue, float positiveMultiple, bool useMaxWhenDtSmall = false)
        {
            if (positiveMultiple < PartialMathUtil.FLOAT_TOLERANCE)
            {
                if (!useMaxWhenDtSmall)
                {
                    return value;
                }
                else
                {
                    return maxValue;
                }
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
        //// incorrect code. TODO: Remove.
        //public static float CalculateClosestMultipleOrClamp(float value, float maxValue, float positiveMultiple,
        //    out float remainingPositiveMultiple,
        //    bool useMaxWhenDtSmall = false)
        //{
        //    if (positiveMultiple < PartialMathUtil.FLOAT_TOLERANCE)
        //    {
        //        if (!useMaxWhenDtSmall)
        //        {
        //            remainingPositiveMultiple = 0f;
        //            return value;
        //        }
        //        else
        //        {
        //            remainingPositiveMultiple = 0f;
        //            return maxValue;
        //        }
        //    }
        //    float difference = maxValue - value;
        //    float absDifference = (float)Math.Abs(difference);
        //    if (positiveMultiple > absDifference)
        //    {
        //        remainingPositiveMultiple = positiveMultiple - absDifference;
        //        return maxValue;
        //    }

        //    float quotient = difference / positiveMultiple;
        //    float remainder = quotient - (int)quotient;
        //    // if lt 10% of the multiple then just return the maxvalue
        //    if (remainder > -0.1f && remainder < 0.1f)
        //    {
        //        remainingPositiveMultiple = 0f;
        //        return maxValue;
        //    }
        //    else
        //    {
        //        remainingPositiveMultiple = 0f;
        //        return maxValue - (remainder * positiveMultiple);
        //    }

        //}
        public static int MoveTrailPosition(int positionIndex, float deltaTime, float localPositionX,
            float localPositionZ, out float newLocalPositionX, out float newLocalPositionZ,
            float[] timeRequiredIncrementalSec, float[] timeRequiredIncrementalVelocityMult,
            (Vector3 worldPosition, Vector3 distanceFromPrev, float localXPosFromPrev)[] worldPositionsPerZUnit,
            float[] localXPositionsPerZUnit,
            float elapsedPositionIndexDeltaTime, out float newElapsedPositionIndexDeltaTime, 
            float worldPositionY, out float newWorldPositionY)
        {
            int i;
            bool addDeltaTime = false;
            float deltaTimeAddRequired = 0f;

            for (i = positionIndex; i < timeRequiredIncrementalSec.Length; i++)
            {
                if (addDeltaTime)
                {
                    deltaTime += deltaTimeAddRequired;
                    deltaTimeAddRequired = 0f;
                    elapsedPositionIndexDeltaTime = 0f;
                    addDeltaTime = false;
                }

                if (elapsedPositionIndexDeltaTime < PartialMathUtil.FLOAT_TOLERANCE && i > 0)
                {
                    localPositionX = PositionUtil.CalculateClosestMultipleOrClamp(localPositionX, localXPositionsPerZUnit[i - 1], deltaTime, useMaxWhenDtSmall: true);
                    localPositionZ = PositionUtil.CalculateClosestMultipleOrClamp(localPositionZ, (float)(i - 1), deltaTime, useMaxWhenDtSmall: true);
                }

                float indexTimeRequiredSec = timeRequiredIncrementalSec[i];

                float calculationElapsedPositionIndexDeltaTime = elapsedPositionIndexDeltaTime;
                bool useMaxWhenDtSmall;
                //Debug.Log($"{elapsedPositionIndexDeltaTime}, {elapsedPositionIndexDeltaTime + deltaTime}, {indexTimeRequiredSec}");
                if (calculationElapsedPositionIndexDeltaTime + deltaTime >= indexTimeRequiredSec)
                {
                    float remainingDeltaTime = indexTimeRequiredSec - calculationElapsedPositionIndexDeltaTime;

                    addDeltaTime = true;
                    deltaTimeAddRequired = deltaTime - remainingDeltaTime;
                    //Debug.Log(deltaTimeAddRequired)
                    
                    // No need to subtract from deltatime, the values are clamped anyways, 
                    // and it responds better with ClosestMultipleOrClamp.
                    // deltaTime = remainingDeltaTime.
                    //Debug.Log($"{deltaTimeAddRequired}, {deltaTime}, {remainingDeltaTime}");
                    calculationElapsedPositionIndexDeltaTime = indexTimeRequiredSec;

                    //Debug.Log($"{deltaTimeAddRequired}, {indexTimeRequiredSec}");
                    useMaxWhenDtSmall = true;

                }
                else
                {
                    calculationElapsedPositionIndexDeltaTime += deltaTime;
                    useMaxWhenDtSmall = false;
                }

                float positionIndexDeltaTimePercentage;
                if (indexTimeRequiredSec > PartialMathUtil.FLOAT_TOLERANCE)
                {
                    positionIndexDeltaTimePercentage = Mathf.Clamp01(calculationElapsedPositionIndexDeltaTime / indexTimeRequiredSec);
                }
                else
                {
                    positionIndexDeltaTimePercentage = 1f;
                }

                // if passed then reset elapsed time for next index.
                if (addDeltaTime)
                {
                    elapsedPositionIndexDeltaTime = 0f;
                }
                else
                {
                    elapsedPositionIndexDeltaTime = calculationElapsedPositionIndexDeltaTime;
                }

                // Warning: timeRequiredIncrementalVelocityMult[i] = 0 due to no speed defined (for example last index).
                // Fixing this would mean setting force setting the next position instead, but that would make this more complicated
                // when actually each index could just have a time greater than 0.
                float dt = deltaTime * timeRequiredIncrementalVelocityMult[i];

                float newLocalX;
                float maxNewLocalZ = localPositionZ + dt;
                float newLocalZ = PositionUtil.CalculateClosestMultipleOrClamp(localPositionZ, maxNewLocalZ, deltaTime, useMaxWhenDtSmall);

                //Debug.Log($"{localPosition.z},{maxNewLocalZ}, {fixedDeltaTimeIncrement}, {newLocalZ}");
                //float zDecimals = zUnits - positionIndex;
                //Vector3 originalVelocity = WorldPositionsPerZUnit[i].distanceFromPrev;

                float localXFromPrev = worldPositionsPerZUnit[i].localXPosFromPrev;

                //newLocalX = LocalPosition.x + localXFromPrev * (EffectsUtil.EaseInOutQuad(zDecimals) * dt * 2f);
                //TODO1: interp from pos to worldPos with closest dt multiple.
                float maxNewLocalX;
                if (i > 0)
                {
                    maxNewLocalX = localXPositionsPerZUnit[i - 1] + localXFromPrev * EffectsUtil.EaseInOutQuad(positionIndexDeltaTimePercentage);
                }
                else
                {
                    maxNewLocalX = localXPositionsPerZUnit[i];
                }
                
                // This might skip some localX iterations.
                newLocalX = PositionUtil.CalculateClosestMultipleOrClamp(localPositionX, maxNewLocalX, deltaTime, useMaxWhenDtSmall);

                // this could be improved.
                worldPositionY += worldPositionsPerZUnit[i].distanceFromPrev.y * dt;
                //Debug.Log($"{elapsedPositionIndexDeltaTime}, {newLocalX}");
                //newLocalX = LocalPosition.x + localXFromPrev * dt;

                localPositionX = newLocalX;
                localPositionZ = newLocalZ;
                
                if (!addDeltaTime)
                {
                    break;
                }
                else
                {
                    deltaTime = 0f;
                }
            }
            newLocalPositionX = localPositionX;
            newLocalPositionZ = localPositionZ;
            newWorldPositionY = worldPositionY;
            newElapsedPositionIndexDeltaTime = elapsedPositionIndexDeltaTime;
            return i;
        }
    }
}
