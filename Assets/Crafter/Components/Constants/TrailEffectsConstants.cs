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
            { BlinkRibbonTrailType.DashBlink, new BlinkRibbonTrailProps(
                velocity: 80f,
                startPositionOffsetsLocal: new Vector3[1]
                {
                    Vector3.zero
                },
                endPositionOffsetsLocal: new Vector3[1]
                {
                    Vector3.zero
                },

                startRotationOffsetsLocal: new Vector3[1]
                {
                    new Vector3(-45f, 0f, 0f)
                },
                widthMultipliers: new float[1]
                {
                    1f
                },
                trailMarkersLocal: new SerializeableArray<Vector3>[1]
                {
                    new SerializeableArray<Vector3>(new Vector3[2]{
                        new Vector3(0.81f, 3.35f, 5.09f),
                        new Vector3(1f, 4.38f, 8.12f)
                    })
                }
                )
            }

        };
    }
    public enum BlinkRibbonTrailType
    {
        Dual,
        DashBlink,
    }
}
