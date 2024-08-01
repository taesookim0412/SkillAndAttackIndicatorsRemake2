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
                localStartPositionOffsets: new Vector3[2] {
                    new Vector3(-0.3f, -0.3f, 0f),
                    new Vector3(0.3f, 0.3f, 0f)
                },
                localEndPositionOffsets: new Vector3[2]
                {
                    new Vector3(-0.3f, -0.3f, 0f),
                    new Vector3(0.3f, 0.3f, 0f)
                },
                localStartRotationOffsets: new Vector3[2]
                {
                    new Vector3(-45f, 0f, 0f),
                    new Vector3(-45f, 0f, 0f)
                }) 
            }
        };
    }
}
