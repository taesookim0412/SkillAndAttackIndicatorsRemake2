using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public class DashParticles : MonoBehaviour
    {
        private static readonly int YVelocityAnimationCurveId = Shader.PropertyToID("_YVelocityAnimationCurve");

        [SerializeField]
        private VisualEffect VisualEffect;
        public void SetYVelocityAnimationCurve(AnimationCurve animationCurve)
        {
            VisualEffect.SetAnimationCurve(YVelocityAnimationCurveId, animationCurve);
        }
    }
}
