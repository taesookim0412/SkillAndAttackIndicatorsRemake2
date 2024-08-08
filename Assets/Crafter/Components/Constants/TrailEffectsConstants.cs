using Assets.Crafter.Components.Models.dco;
using Assets.Crafter.Components.Models.dpo.TrailEffectsDpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Constants
{
    public static class TrailEffectsConstants
    {
        public static readonly int BlinkRibbonTrailTypeEnumLength = Enum.GetNames(typeof(BlinkRibbonTrailType)).Length;
        public static readonly Dictionary<BlinkRibbonTrailType, BlinkRibbonTrailProps> BlinkRibbonTrailProps = new Dictionary<BlinkRibbonTrailType, BlinkRibbonTrailProps>(BlinkRibbonTrailTypeEnumLength)
        {
            { BlinkRibbonTrailType.Dual, new BlinkRibbonTrailProps(
                startPositionOffsetsLocal: new Vector3[2] {
                    new Vector3(0.39f, -0.13f, 0f),
                    new Vector3(-0.8f, 0f, 0f)
                },
                endPositionOffsetsLocal: new Vector3[2]
                {
                    new Vector3(-0.76f, -0.11f, 0f),
                    new Vector3(-0.2f, 0.04f, 0f)
                },
                startRotationOffsetsLocal: new Vector3[2]
                {
                    new Vector3(-1.41f, 26.2f, 0f),
                    new Vector3(0f, 19.84f, 0f)
                },
                widthMultipliers: new float[2]
                {
                    0.15f, 0.15f
                },
                trailMarkersLocal: new SerializeableArray<Vector3>[2]
                {
                    new SerializeableArray<Vector3>(new Vector3[3]{ 
                        new Vector3(0.99f,0f,2.19f),
                        new Vector3(1.01f,0f,6.06f),
                        new Vector3(0.56f,0f,9.59f) }),
                    new SerializeableArray<Vector3>(new Vector3[3]{ 
                        new Vector3(-0.01f, 0f, 2.19f),
                        new Vector3(1.005f, 0f, 6.06f),
                        new Vector3(0.85f, 0f, 9.59f) })
                }
                )
            }
        };
    }
    public enum BlinkRibbonTrailType
    {
        Dual
    }
}
