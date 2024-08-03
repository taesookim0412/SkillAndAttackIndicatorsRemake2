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
        public int NumTrailMarkers;
        public Vector3[] TrailMarkersLocal;

        public BlinkRibbonTrailProps(int numTrails, Vector3[] startPositionOffsetsLocal, Vector3[] endPositionOffsetsLocal, Vector3[] startRotationOffsetsLocal, float[] widthMultipliers,
            int numTrailMarkers, Vector3[] trailMarkersLocal)
        {
            NumTrails = numTrails;
            StartPositionOffsetsLocal = startPositionOffsetsLocal;
            EndPositionOffsetsLocal = endPositionOffsetsLocal;
            StartRotationOffsetsLocal = startRotationOffsetsLocal;
            WidthMultipliers = widthMultipliers;
            NumTrailMarkers = numTrailMarkers;
            TrailMarkersLocal = trailMarkersLocal;
            
        }

        public BlinkRibbonTrailProps(Vector3[] startPositionOffsetsLocal, Vector3[] endPositionOffsetsLocal, Vector3[] startRotationOffsetsLocal, float[] widthMultipliers,
            Vector3[] trailMarkersLocal)
        {
            NumTrails = startPositionOffsetsLocal.Length;
            StartPositionOffsetsLocal = startPositionOffsetsLocal;
            EndPositionOffsetsLocal = endPositionOffsetsLocal;
            StartRotationOffsetsLocal = startRotationOffsetsLocal;
            WidthMultipliers = widthMultipliers;
            NumTrailMarkers = trailMarkersLocal.Length;
            TrailMarkersLocal = trailMarkersLocal;
        }
    }
    public enum BlinkRibbonTrailType
    {
        Dual
    }
}
