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
    public class RotatingCoordinateVector3Angles
    {
        public bool YRotationOnly;
        public float CosZAngle;
        public float SinZAngle;
        public float CosXAngle;
        public float SinXAngle;
        public float CosYAngle;
        public float SinYAngle;

        public RotatingCoordinateVector3Angles(Vector3 rotationVector) 
        {
            SetRotationVector(rotationVector);
        }
        public void SetRotationVector(Vector3 rotationVector)
        {
            bool xAngleExists = rotationVector.x < PartialMathUtil.FLOAT_TOLERANCE_NEGATIVE || rotationVector.x > PartialMathUtil.FLOAT_TOLERANCE;
            bool yAngleExists = rotationVector.y < PartialMathUtil.FLOAT_TOLERANCE_NEGATIVE || rotationVector.y > PartialMathUtil.FLOAT_TOLERANCE;
            bool zAngleExists = rotationVector.z < PartialMathUtil.FLOAT_TOLERANCE_NEGATIVE || rotationVector.z > PartialMathUtil.FLOAT_TOLERANCE;
            if (xAngleExists)
            {
                CosXAngle = (float)Math.Cos(rotationVector.x * PartialMathUtil.Deg2Rad);
                SinXAngle = (float)Math.Sin(rotationVector.x * PartialMathUtil.Deg2Rad);
            }
            else
            {
                CosXAngle = 1f;
                SinXAngle = 0f;
            }
            if (yAngleExists)
            {
                CosYAngle = (float)Math.Cos(rotationVector.y * PartialMathUtil.Deg2Rad);
                SinYAngle = (float)Math.Sin(rotationVector.y * PartialMathUtil.Deg2Rad);
            }
            else
            {
                CosYAngle = 1f;
                SinYAngle = 0f;
            }
            if (zAngleExists)
            {
                CosZAngle = (float)Math.Cos(rotationVector.z * PartialMathUtil.Deg2Rad);
                SinZAngle = (float)Math.Sin(rotationVector.z * PartialMathUtil.Deg2Rad);
            }
            else
            {
                CosZAngle = 1f;
                SinZAngle = 0f;
            }
            if (!(xAngleExists || zAngleExists))
            {
                YRotationOnly = true;
            }
            else
            {
                YRotationOnly = false;
            }
        }
        public RotatingCoordinateVector3Angles(float yAngle)
        {
            YRotationOnly = true;
            CosZAngle = 1f;
            SinZAngle = 0f;
            CosXAngle = 1f;
            SinXAngle = 0f;
            CosYAngle = (float)Math.Cos(yAngle * PartialMathUtil.Deg2Rad);
            SinYAngle = (float)Math.Sin(yAngle * PartialMathUtil.Deg2Rad);
        }

        public RotatingCoordinateVector3Angles(float xAngle, float yAngle, float zAngle)
        {
            CosZAngle = (float)Math.Cos(zAngle * PartialMathUtil.Deg2Rad);
            SinZAngle = (float)Math.Sin(zAngle * PartialMathUtil.Deg2Rad);
            CosXAngle = (float)Math.Cos(xAngle * PartialMathUtil.Deg2Rad);
            SinXAngle = (float)Math.Sin(xAngle * PartialMathUtil.Deg2Rad);
            CosYAngle = (float)Math.Cos(yAngle * PartialMathUtil.Deg2Rad);
            SinYAngle = (float)Math.Sin(yAngle * PartialMathUtil.Deg2Rad);
        }
    }
}
