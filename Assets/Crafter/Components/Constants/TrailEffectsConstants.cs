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
                velocity: 80f,
                startPositionOffsetsLocal: new Vector3[2] {
                    new Vector3(0.2f, 0f, 0f),
                    new Vector3(-0.2f, 0f, 0f)
                },
                endPositionOffsetsLocal: new Vector3[2]
                {
                    new Vector3(0.2f, 0f, 0f),
                    new Vector3(-0.2f, 0f, 0f)
                },
                startRotationOffsetsLocal: new Vector3[2]
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(0f, 0f, 0f)
                },
                widthMultipliers: new float[2]
                {
                    0.85f, 0.85f
                },
                trailMarkersLocal: new SerializeableArray<Vector3>[2]
                {
                    new SerializeableArray<Vector3>(new Vector3[3]{
                        new Vector3(0.2f,1.3f,0.11f),
                        new Vector3(0.45f, 1.3f, 5.6f),
                        new Vector3(0.2f, 1.3f, 9.59f) }),
                    new SerializeableArray<Vector3>(new Vector3[3]{
                        new Vector3(-0.2f,1.3f,0.11f),
                        new Vector3(-0.45f, 1.3f, 5.6f),
                        new Vector3(-0.2f, 1.3f, 9.59f) })
                }
                )
            },
            { BlinkRibbonTrailType.DashBlinkStart, new BlinkRibbonTrailProps(
                velocity: 15f,
                startPositionOffsetsLocal: new Vector3[1]
                {
                    new Vector3(0f, 0f, 0.2f)
                },
                endPositionOffsetsLocal: new Vector3[1]
                {
                    new Vector3(1f, 1.135f, 1.76f)
                },

                startRotationOffsetsLocal: new Vector3[1]
                {
                    new Vector3(-180f, 0f, 0f)
                },
                widthMultipliers: new float[1]
                {
                    1f
                },
                trailMarkersLocal: new SerializeableArray<Vector3>[1]
                {
                    new SerializeableArray<Vector3>(new Vector3[5]{
                        new Vector3(0.202f, 0.059f, 0.258f),
                        new Vector3(0.438f, 0.516f, 0.422f),
                        new Vector3(0.579f, 0.895f, 0.572f),
                        new Vector3(0.684f, 1.281f, 0.63f),
                        new Vector3(1f, 1.9f, 1.2f)

                    })
                }
                )
            },
            { BlinkRibbonTrailType.DashBlinkEnd, new BlinkRibbonTrailProps(
                velocity: 15f,
                startPositionOffsetsLocal: new Vector3[1]
                {
                    new Vector3(-11f, 1.6f, -1.1f)
                    
                },
                endPositionOffsetsLocal: new Vector3[1]
                {
                    new Vector3(0f, 0f, -0.2f)
                },
                startRotationOffsetsLocal: new Vector3[1]
                {
                    new Vector3(0f, 0f, 0f)
                },
                widthMultipliers: new float[1]
                {
                    1f
                },
                trailMarkersLocal: new SerializeableArray<Vector3>[1]
                {
                    new SerializeableArray<Vector3>(new Vector3[1]{ new Vector3(-0.8f, 0.8f, -1.2f) })
                }
                )
            }

        };
    }
    public enum BlinkRibbonTrailType
    {
        Dual,
        DashBlinkStart,
        DashBlinkEnd
    }
}
