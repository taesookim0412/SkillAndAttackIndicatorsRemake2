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
                    0.15f, 0.15f
                },
                trailMarkersLocal: new SerializeableArray<Vector3>[2]
                {
                    new SerializeableArray<Vector3>(new Vector3[4]{ 
                        new Vector3(0.2f,0.4f,0f),
                        new Vector3(0.2f, 1.37f, 2.31f),
                        new Vector3(0.2f, 1.938f, 5.98f),
                        new Vector3(0.2f, 1.8f, 9.59f) }),
                    new SerializeableArray<Vector3>(new Vector3[4]{ 
                        new Vector3(-0.2f,0.4f,0f),
                        new Vector3(-0.2f, 1.37f, 2.31f),
                        new Vector3(-0.2f, 1.938f, 5.98f),
                        new Vector3(-0.2f, 1.8f, 9.59f)})
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
