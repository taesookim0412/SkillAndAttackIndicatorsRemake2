using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Crafter.Components.Models
{
    /// <summary>
    /// Handles XY rotations, returns Vector3, and only does unit rotations.
    /// </summary>
    public struct RotatingCoordinateXYVector3Angles
    {
        public float CosXAngle;
        public float SinXAngle;
        public float CosYAngle;
        public float SinYAngle;

        public RotatingCoordinateXYVector3Angles(Vector3 rotationVector) : this(rotationVector.x, rotationVector.y)
        {

        }

        public RotatingCoordinateXYVector3Angles(float xAngle, float yAngle)
        {
            if (xAngle < PartialMathUtil.FLOAT_TOLERANCE_NEGATIVE || xAngle > PartialMathUtil.FLOAT_TOLERANCE)
            {
                CosXAngle = (float)Math.Cos(xAngle * PartialMathUtil.Deg2Rad);
                SinXAngle = (float)Math.Sin(xAngle * PartialMathUtil.Deg2Rad);
            }
            else
            {
                CosXAngle = 1f;
                SinXAngle = 0f;
            }
            if (yAngle < PartialMathUtil.FLOAT_TOLERANCE_NEGATIVE || yAngle > PartialMathUtil.FLOAT_TOLERANCE)
            {
                CosYAngle = (float)Math.Cos(yAngle * PartialMathUtil.Deg2Rad);
                SinYAngle = (float)Math.Sin(yAngle * PartialMathUtil.Deg2Rad);
            }
            else
            {
                CosYAngle = 1f;
                SinYAngle = 0f;
            }
        }
        public Vector3 RotateX_Forward()
        {
            float new_y = 0f - SinXAngle;
            float new_z = CosXAngle;

            return new Vector3(0f, new_y, new_z);
        }
        public Vector3 RotateY_Forward()
        {
            float new_x = SinYAngle;
            float new_z = CosYAngle;

            return new Vector3(new_x, 0f, new_z);
        }
        public Vector3 RotateXY_Forward()
        {
            float new_y = 0f - SinXAngle;
            float new_z = CosXAngle;

            float new_x_2 = new_z * SinYAngle;
            float new_z_2 = new_z * CosYAngle;

            return new Vector3(new_x_2, new_y, new_z_2);
        }
        public Vector3 RotateXY_Forward(float distance)
        {
            float new_y = 0f - distance * SinXAngle;
            float new_z = distance * CosXAngle;

            float new_z_2 = new_z * CosYAngle;
            float new_x_2 = new_z * SinYAngle;

            return new Vector3(new_x_2, new_y, new_z_2);
        }
    }
}
