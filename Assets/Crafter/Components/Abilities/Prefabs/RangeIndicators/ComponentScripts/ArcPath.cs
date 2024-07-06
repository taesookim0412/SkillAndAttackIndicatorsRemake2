﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public class ArcPath : AbstractAbilityFX
    {
        private static readonly int StartYOffsetId = Shader.PropertyToID("_StartYOffset");
        private static readonly int EndYOffsetId = Shader.PropertyToID("_EndYOffset");

        public float LocalPositionX;
        public float LocalPositionZ;

        public void SetLocalPositionFields(float localPositionX, float localPositionZ)
        {
            LocalPositionX = localPositionX;
            LocalPositionZ = localPositionZ;
        }

        public void SetOffsetFX(float startYOffset, float endYOffset)
        {
            VisualEffect.SetFloat(StartYOffsetId, startYOffset);
            VisualEffect.SetFloat(EndYOffsetId, endYOffset);
        }
        public void ResetFX()
        {
            VisualEffect.SetFloat(StartYOffsetId, 0f);
            VisualEffect.SetFloat(EndYOffsetId, 0f);
        }
        
    }
}
