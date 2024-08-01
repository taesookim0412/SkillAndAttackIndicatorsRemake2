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
        public Vector3[] LocalStartPositionOffsets;
        public Vector3[] LocalEndPositionOffsets;
        public Vector3[] LocalStartRotationOffsets;

        public BlinkRibbonTrailProps(int numTrails, Vector3[] localStartPositionOffsets, Vector3[] localEndPositionOffsets, Vector3[] localStartRotationOffsets)
        {
            NumTrails = numTrails;
            LocalStartPositionOffsets = localStartPositionOffsets;
            LocalEndPositionOffsets = localEndPositionOffsets;
            LocalStartRotationOffsets = localStartRotationOffsets;
        }

        public BlinkRibbonTrailProps(Vector3[] localStartPositionOffsets, Vector3[] localEndPositionOffsets, Vector3[] localStartRotationOffsets)
        {
            NumTrails = localStartPositionOffsets.Length;
            LocalStartPositionOffsets = localStartPositionOffsets;
            LocalEndPositionOffsets = localEndPositionOffsets;
            LocalStartRotationOffsets = localStartRotationOffsets;
        }
    }
    public enum BlinkRibbonTrailType
    {
        Dual
    }
}
