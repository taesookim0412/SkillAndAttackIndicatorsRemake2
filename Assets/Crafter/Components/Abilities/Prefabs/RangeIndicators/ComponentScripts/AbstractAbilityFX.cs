﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public abstract class AbstractAbilityFX: MonoBehaviour
    {
        [SerializeField]
        protected VisualEffect VisualEffect;

        [SerializeField]
        protected ParticleSystem[] ParticleSystems;

        [SerializeField]
        public TrailRenderer TrailRenderer;

        public void DisableParticleSystems()
        {
            foreach (ParticleSystem particleSystem in ParticleSystems)
            {
                particleSystem.enableEmission = false;
            }
        }
        public void EnableParticleSystems()
        {
            foreach (ParticleSystem particleSystem in ParticleSystems)
            {
                particleSystem.enableEmission = true;
            }
        }
        public void SetTrailRendererWidth(float widthMultiplier)
        {
            TrailRenderer.widthMultiplier = widthMultiplier;
        }

        public virtual void CleanUpInstance()
        {

        }
    }
}
