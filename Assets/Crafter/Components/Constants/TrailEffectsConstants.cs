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
                    new Vector3(-0.3f, -0.3f, 0f),
                    new Vector3(0.3f, 0.3f, 0f)
                },
                endPositionOffsetsLocal: new Vector3[2]
                {
                    new Vector3(-0.3f, -0.3f, 0f),
                    new Vector3(0.3f, 0.3f, 0f)
                },
                startRotationOffsetsLocal: new Vector3[2]
                {
                    new Vector3(-45f, 0f, 0f),
                    new Vector3(-45f, 0f, 0f)
                },
                widthMultipliers: new float[2]
                {
                    0.1f, 0.1f
                },
                trailMarkersLocal: new SerializeableArray<Vector3>[2]
                {
                    new SerializeableArray<Vector3>(new Vector3[3]{ new Vector3(1f,1f,1f),
                        new Vector3(2f,1f,2f),
                        new Vector3(3f,1f,3f) }),
                    new SerializeableArray<Vector3>(new Vector3[3]{ new Vector3(1f,1f,1f),
                        new Vector3(2f,1f,2f),
                        new Vector3(3f,1f,3f) })
                }
                )
            }
        };
    }
}
