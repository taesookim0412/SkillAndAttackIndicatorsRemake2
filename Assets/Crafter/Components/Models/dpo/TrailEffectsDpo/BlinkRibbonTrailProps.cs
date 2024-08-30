using Assets.Crafter.Components.Models.dco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Models.dpo.TrailEffectsDpo
{
    [Serializable]
    public class BlinkRibbonTrailProps
    {
        public float Velocity;
        public int NumTrails;
        public Vector3[] StartPositionOffsetsLocal;
        public Vector3[] EndPositionOffsetsLocal;
        public Vector3[] StartRotationOffsetsLocal;
        public float[] WidthMultipliers;
        public int[] NumTrailMarkers;
        public SerializeableArray<Vector3>[] TrailMarkersLocal;

        public BlinkRibbonTrailProps(float velocity, int numTrails, Vector3[] startPositionOffsetsLocal, Vector3[] endPositionOffsetsLocal, Vector3[] startRotationOffsetsLocal, float[] widthMultipliers,
            int[] numTrailMarkers, SerializeableArray<Vector3>[] trailMarkersLocal)
        {
            Velocity = velocity;
            NumTrails = numTrails;
            StartPositionOffsetsLocal = startPositionOffsetsLocal;
            EndPositionOffsetsLocal = endPositionOffsetsLocal;
            StartRotationOffsetsLocal = startRotationOffsetsLocal;
            WidthMultipliers = widthMultipliers;
            NumTrailMarkers = numTrailMarkers;
            TrailMarkersLocal = trailMarkersLocal;
            
        }

        public BlinkRibbonTrailProps(float velocity, Vector3[] startPositionOffsetsLocal, Vector3[] endPositionOffsetsLocal, Vector3[] startRotationOffsetsLocal, float[] widthMultipliers,
            SerializeableArray<Vector3>[] trailMarkersLocal)
        {
            Velocity = velocity;
            NumTrails = startPositionOffsetsLocal.Length;
            StartPositionOffsetsLocal = startPositionOffsetsLocal;
            EndPositionOffsetsLocal = endPositionOffsetsLocal;
            StartRotationOffsetsLocal = startRotationOffsetsLocal;
            WidthMultipliers = widthMultipliers;
            int[] numTrailMarkers = new int[trailMarkersLocal.Length];
            for (int i = 0; i < numTrailMarkers.Length; i++)
            {
                numTrailMarkers[i] = trailMarkersLocal[i].Items.Length;
            }
            NumTrailMarkers = numTrailMarkers;
            TrailMarkersLocal = trailMarkersLocal;
        }
    }

}
