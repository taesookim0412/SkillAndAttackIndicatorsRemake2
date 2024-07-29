using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public static class Vector3Util
    {

        public static Vector3 LookRotationPitchYaw(this Vector3 target)
        {
            float directionX = target.x;
            float directionY = target.y;
            float directionZ = target.z;
            // Calculate pitch (rotation around the horizontal X-axis)
            float pitch = (float)Math.Atan2(0f - directionY, Math.Sqrt(directionX * directionX + directionZ * directionZ));

            // Calculate yaw (rotation around the vertical Y-axis)
            float yaw = (float)Math.Atan2(directionX, directionZ);

            float pitchDegrees = PartialMathUtil.RepeatRotation(pitch * PartialMathUtil.Rad2Deg);
            // Calculate yaw and pitch in degrees
            float yawDegrees = PartialMathUtil.RepeatRotation(yaw * PartialMathUtil.Rad2Deg);

            // Return the rotation as a Vector3
            return new Vector3(pitchDegrees, yawDegrees, 0f);
        }
    }
}
