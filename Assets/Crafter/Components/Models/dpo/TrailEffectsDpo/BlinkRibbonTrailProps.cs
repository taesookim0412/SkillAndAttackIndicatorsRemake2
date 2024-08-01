using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Models.dpo.TrailEffectsDpo
{
    public class BlinkRibbonTrailProps
    {
        public Vector3[] LocalStartPositionOffsets;
        public Vector3[] LocalEndPositionOffsets;
        public Vector3[] LocalStartRotationOffsets;

        public BlinkRibbonTrailProps(Vector3[] localStartPositionOffsets, Vector3[] localEndPositionOffsets, Vector3[] localStartRotationOffsets)
        {
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
