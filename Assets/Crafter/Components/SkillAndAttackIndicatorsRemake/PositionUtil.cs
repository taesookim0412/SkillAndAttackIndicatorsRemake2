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
        public static float CalculateClosestMultipleOrClamp(float value, float maxValue, float positiveMultiple, bool useMaxWhenDtSmall)
        {
            if (positiveMultiple < SkillAndAttackIndicatorSystem.FLOAT_TOLERANCE)
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
        public static int MoveTrailPosition(int positionIndex, float deltaTime, float localPositionX,
            float localPositionZ, out float newLocalPositionX, out float newLocalPositionZ,
            float[] timeRequiredIncrementalSec, float[] timeRequiredIncrementalVelocityMult,
            (Vector3 worldPosition, Vector3 distanceFromPrev, float localXPosFromPrev)[] worldPositionsPerZUnit,
            float[] localXPositionsPerZUnit,
            ref float elapsedPositionIndexDeltaTimeRef, float worldPositionY, out float newWorldPositionY)
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
                    addDeltaTime = false;
                }

                float indexTimeRequiredSec = timeRequiredIncrementalSec[i];

                // set elapsedPositionIndexDeltaTime and set remainder delta times.
                float elapsedPositionIndexDeltaTime = elapsedPositionIndexDeltaTimeRef;
                bool useMaxWhenDtSmall;
                //Debug.Log($"{elapsedPositionIndexDeltaTime}, {elapsedPositionIndexDeltaTime + deltaTime}, {indexTimeRequiredSec}");
                if (elapsedPositionIndexDeltaTime + deltaTime >= indexTimeRequiredSec)
                {
                    float remainingDeltaTime = indexTimeRequiredSec - elapsedPositionIndexDeltaTime;

                    addDeltaTime = true;
                    deltaTimeAddRequired = deltaTime - remainingDeltaTime;
                    //Debug.Log(deltaTimeAddRequired)
                    
                    // No need to subtract from deltatime, the values are clamped anyways, 
                    // and it responds better with ClosestMultipleOrClamp.
                    // deltaTime = remainingDeltaTime.
                    //Debug.Log($"{deltaTimeAddRequired}, {deltaTime}, {remainingDeltaTime}");
                    elapsedPositionIndexDeltaTime = indexTimeRequiredSec;

                    //Debug.Log($"{deltaTimeAddRequired}, {indexTimeRequiredSec}");
                    useMaxWhenDtSmall = true;

                }
                else
                {
                    elapsedPositionIndexDeltaTime += deltaTime;
                    useMaxWhenDtSmall = false;
                }

                float positionIndexDeltaTimePercentage;
                if (indexTimeRequiredSec > SkillAndAttackIndicatorSystem.FLOAT_TOLERANCE)
                {
                    positionIndexDeltaTimePercentage = Mathf.Clamp01(elapsedPositionIndexDeltaTime / indexTimeRequiredSec);
                }
                else
                {
                    positionIndexDeltaTimePercentage = 1f;
                }

                if (addDeltaTime && i != timeRequiredIncrementalSec.Length - 1)
                {
                    elapsedPositionIndexDeltaTime = 0f;
                }

                

                if (elapsedPositionIndexDeltaTime < SkillAndAttackIndicatorSystem.FLOAT_TOLERANCE && i > 0)
                {
                    localPositionX = PositionUtil.CalculateClosestMultipleOrClamp(localPositionX, localXPositionsPerZUnit[i - 1], deltaTime, useMaxWhenDtSmall);
                    localPositionZ = PositionUtil.CalculateClosestMultipleOrClamp(localPositionZ, (float)(i - 1), deltaTime, useMaxWhenDtSmall);
                }

                elapsedPositionIndexDeltaTimeRef = elapsedPositionIndexDeltaTime;

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
            return i;
        }
    }
}
