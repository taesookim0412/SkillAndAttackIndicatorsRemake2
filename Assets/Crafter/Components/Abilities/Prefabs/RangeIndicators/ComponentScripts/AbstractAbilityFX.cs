using System;
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

        [HideInInspector]
        public bool Active = false;
        [HideInInspector]
        public bool Completed = false;
        protected void Initialize()
        {
            Active = false;
            Completed = false;
        }
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
        public void ClearParticleSystems()
        {
            foreach (ParticleSystem particleSystem in ParticleSystems)
            {
                particleSystem.Clear();
            }
        }
        protected void ClearTrailRenderer()
        {
            TrailRenderer.Clear();
        }
        // Disable Particle Systems after the FX is completed.
        public virtual void Complete()
        {
            Completed = true;
        }
        // Clean up / destroy resources / arrays after the observer is completed.
        public virtual void CleanUpInstance()
        {

        }
    }
}
