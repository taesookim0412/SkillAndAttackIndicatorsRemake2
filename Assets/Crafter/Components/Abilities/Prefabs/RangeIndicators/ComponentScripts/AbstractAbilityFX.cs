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

        [SerializeField]
        public bool EnableParticleSystemsRequired = false;
        [NonSerialized, HideInInspector]
        public bool Active = false;
        [NonSerialized, HideInInspector]
        public bool Completed = false;
        [NonSerialized, HideInInspector]
        public bool CompletedStateful = false;
        protected void Initialize()
        {
            Active = false;
            Completed = false;
            CompletedStateful = false;
        }
        public void EnableSystems()
        {
            if (EnableParticleSystemsRequired)
            {
                EnableParticleSystems();
            }
        }
        public void DisableSystems()
        {
            if (EnableParticleSystemsRequired)
            {
                DisableParticleSystems();
            }
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
        public void ReinitVisualEffect()
        {
            VisualEffect.Reinit();
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
        // Enforce the state is the same at the end. This can be very complex so the chain responsible for the state should usually only have this member.
        public virtual void CompleteStatefulFX()
        {
            CompletedStateful = true;
        }
        // Set resources to null after observer is completed.
        public virtual void CleanUpInstance()
        {

        }
    }
}
