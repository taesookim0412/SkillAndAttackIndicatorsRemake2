using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Models.dpo.TrailEffectsDpo
{
    [Serializable]
    public class BlinkRibbonTrailProps
    {
        public int NumTrails;
        public Vector3[] StartPositionOffsetsLocal;
        public Vector3[] EndPositionOffsetsLocal;
        public Vector3[] StartRotationOffsetsLocal;
        public float[] WidthMultipliers;

        public BlinkRibbonTrailProps(int numTrails, Vector3[] startPositionOffsetsLocal, Vector3[] endPositionOffsetsLocal, Vector3[] startRotationOffsetsLocal, float[] widthMultipliers)
        {
            NumTrails = numTrails;
            StartPositionOffsetsLocal = startPositionOffsetsLocal;
            EndPositionOffsetsLocal = endPositionOffsetsLocal;
            StartRotationOffsetsLocal = startRotationOffsetsLocal;
            WidthMultipliers = widthMultipliers;
        }

        public BlinkRibbonTrailProps(Vector3[] startPositionOffsetsLocal, Vector3[] endPositionOffsetsLocal, Vector3[] startRotationOffsetsLocal, float[] widthMultipliers)
        {
            NumTrails = startPositionOffsetsLocal.Length;
            StartPositionOffsetsLocal = startPositionOffsetsLocal;
            EndPositionOffsetsLocal = endPositionOffsetsLocal;
            StartRotationOffsetsLocal = startRotationOffsetsLocal;
            WidthMultipliers = widthMultipliers;
        }
    }
    public enum BlinkRibbonTrailType
    {
        Dual
    }
}
