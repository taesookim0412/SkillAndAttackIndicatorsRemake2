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
        private static readonly int YVelocityAnimCurveId = Shader.PropertyToID("_YVelocityAnimationCurve");
        private static readonly int XAngleId = Shader.PropertyToID("_XAngle");

        [SerializeField]
        private VisualEffect VisualEffect;
        public void SetYVelocityAnimationCurve(AnimationCurve animationCurve)
        {
            VisualEffect.SetAnimationCurve(YVelocityAnimCurveId, animationCurve);
        }
        public void SetXAngle(float xAngle)
        {
            VisualEffect.SetFloat(XAngleId, xAngle);
        }
    }
}
