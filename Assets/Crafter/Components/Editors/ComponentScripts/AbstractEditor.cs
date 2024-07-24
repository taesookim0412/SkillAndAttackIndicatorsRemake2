﻿using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Instance = (T)target;

            Undo.RecordObject(Instance, "Editor State");

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            SkipDestroy = GUILayout.Toggle(SkipDestroy, "SkipDestroy");

            if (GUILayout.Button("Restart"))
            {
                OnDisable();
                ParticleSystem particleSystem = Instance.GetComponent<ParticleSystem>();
                GameObject.DestroyImmediate(particleSystem);
            }
        }
        protected void Initialize()
        {
            if (!VariablesSet && PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                T instance = (T)target;
                Instance = instance;
                if (OnInitialize(instance))
                {
                    VariablesSet = true;
                    VariablesAdded = true;
                }
            }
        }
        protected abstract bool OnInitialize(T instance);
        protected abstract void ManualUpdate();
        protected abstract void EditorDestroy();
        public void OnDisable()
        {
            if (!SkipDestroy && VariablesAdded)
            {
                EditorDestroy();
                VariablesSet = false;
                VariablesAdded = false;
            }
        }
        public void OnSceneGUI()
        {
            Initialize();
            if (VariablesSet)
            {
                ObserverUpdateCache.Update_FixedUpdate();
                ManualUpdate();
            }
        }
        protected void SetObserverUpdateCache()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ObserverUpdateCache = new ObserverUpdateCache(newTime);
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
