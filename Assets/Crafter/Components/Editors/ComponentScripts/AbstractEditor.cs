using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Crafter.Components.Editors.ComponentScripts
{
    public abstract class AbstractEditor<T> : Editor where T: AbstractAbilityFXBuilder
    {
        protected T Instance;
        protected ObserverUpdateCache ObserverUpdateCache;
        protected bool VariablesSet;
        protected bool VariablesAdded;
        protected bool SkipDestroy = false;

        protected void Initialize()
        {
            if (!VariablesSet && PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                if (OnInitialize())
                {
                    VariablesSet = true;
                    VariablesAdded = true;
                }
            }
        }
        protected abstract bool OnInitialize();
        public void OnDisable()
        {
            if (!SkipDestroy && VariablesAdded)
            {
                Instance.EditorDestroy();
                VariablesSet = false;
                VariablesAdded = false;
            }
        }
        public void OnSceneGUI()
        {
            Initialize();
            if (VariablesSet)
            {
                long previousUpdateTickTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                ObserverUpdateCache.Update_FixedUpdate();
                Instance.ManualUpdate();
            }
        }
        protected void SetObserverUpdateCache()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ObserverUpdateCache = new ObserverUpdateCache(newTime);
        }
        protected void UpdateObserverUpdateCache_FixedUpdate()
        {
            ObserverUpdateCache.Update_FixedUpdate();
        }

        protected void TryAddParticleSystem(GameObject instance)
        {
            if (instance.GetComponent<ParticleSystem>() == null)
            {
                ParticleSystem particleSystem = instance.AddComponent<ParticleSystem>();
                ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
                emissionModule.enabled = false;

                ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
                shapeModule.enabled = false;
            }
        }
    }
}
