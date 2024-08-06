using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Models.dpo.TrailEffectsDpo
{
    [Serializable]
    public class BlinkRibbonTrailProps: ISerializationCallbackReceiver
    {
        public int NumTrails;
        public Vector3[] StartPositionOffsetsLocal;
        public Vector3[] EndPositionOffsetsLocal;
        public Vector3[] StartRotationOffsetsLocal;
        public float[] WidthMultipliers;
        public int[] NumTrailMarkers;
        public Vector3[][] TrailMarkersLocal;

        private Vector3[] _serializedTrailMarkersLocal;
        private int[] _serializedNumTrailMarkers;

        public BlinkRibbonTrailProps(int numTrails, Vector3[] startPositionOffsetsLocal, Vector3[] endPositionOffsetsLocal, Vector3[] startRotationOffsetsLocal, float[] widthMultipliers,
            int[] numTrailMarkers, Vector3[][] trailMarkersLocal)
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
            Vector3[][] trailMarkersLocal)
        {
            NumTrails = startPositionOffsetsLocal.Length;
            StartPositionOffsetsLocal = startPositionOffsetsLocal;
            EndPositionOffsetsLocal = endPositionOffsetsLocal;
            StartRotationOffsetsLocal = startRotationOffsetsLocal;
            WidthMultipliers = widthMultipliers;
            int[] numTrailMarkers = new int[trailMarkersLocal.Length];
            for (int i = 0; i < numTrailMarkers.Length; i++)
            {
                numTrailMarkers[i] = trailMarkersLocal[i].Length;
            }
            NumTrailMarkers = numTrailMarkers;
            TrailMarkersLocal = trailMarkersLocal;
        }

        public void OnBeforeSerialize()
        {
            int totalLength = 0;

            Vector3[] serializedTrailMarkersLocal = null;
            int[] serializedNumTrailMarkers = null;
            if (TrailMarkersLocal != null)
            {
                serializedNumTrailMarkers = new int[TrailMarkersLocal.Length];
                for (int i = 0; i < TrailMarkersLocal.Length; i++)
                {
                    if (TrailMarkersLocal[i] != null)
                    {
                        int trailMarkers = TrailMarkersLocal[i].Length;
                        serializedNumTrailMarkers[i] = trailMarkers;
                        totalLength += trailMarkers;
                    }
                }

                serializedTrailMarkersLocal = new Vector3[totalLength];

                int currentIndex = 0;
                for (int i = 0; i < TrailMarkersLocal.Length; i++)
                {
                    if (TrailMarkersLocal[i] != null)
                    {
                        for (int j = 0; j < TrailMarkersLocal[i].Length; j++)
                        {
                            serializedTrailMarkersLocal[currentIndex++] = TrailMarkersLocal[i][j];
                        }
                    }
                }
            }

            _serializedTrailMarkersLocal = serializedTrailMarkersLocal;
            _serializedNumTrailMarkers = serializedNumTrailMarkers;
        }

        public void OnAfterDeserialize()
        {
            if (_serializedNumTrailMarkers != null && _serializedTrailMarkersLocal != null)
            {
                Vector3[][] trailMarkersLocal = new Vector3[_serializedNumTrailMarkers.Length][];
                int serializedTrailMarkersIndex = 0;
                for (int i = 0; i < _serializedNumTrailMarkers.Length; i++)
                {
                    Vector3[] trailTrailMarkersLocal = new Vector3[_serializedNumTrailMarkers[i]];
                    for (int j = 0; j < _serializedNumTrailMarkers[i]; j++)
                    {
                        trailTrailMarkersLocal[j] = _serializedTrailMarkersLocal[serializedTrailMarkersIndex++];
                    }
                    trailMarkersLocal[i] = trailTrailMarkersLocal;
                }
                TrailMarkersLocal = trailMarkersLocal;
            }
        }
    }
    public enum BlinkRibbonTrailType
    {
        Dual
    }
}
