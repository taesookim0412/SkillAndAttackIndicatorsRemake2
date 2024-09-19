using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
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
        protected bool InitializeFailed;
        protected bool VariablesAdded;
        protected bool SkipDestroy = false;

        private int WarnIncrement = 0;
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
            }
        }
        protected void Initialize(ObserverUpdateCache observerUpdateCache)
        {
            if (!InitializeFailed && !VariablesSet && PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                T instance = (T)target;
                Instance = instance;
                try
                {
                    if (OnInitialize(instance, observerUpdateCache))
                    {
                        VariablesSet = true;
                        VariablesAdded = true;
                    }
                }
                catch (Exception e) 
                {
                    Debug.LogError($"Initialize Failed.");
                    Debug.LogError(e);
                    InitializeFailed = true;
                }
            }
        }
        protected abstract bool OnInitialize(T instance, ObserverUpdateCache observerUpdateCache);
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
            Initialize(null);
            if (VariablesSet)
            {
                if (UpdateRenderThread())
                {
                    WarnFixedUpdateTimeChanged();
                    ManualUpdate();
                }
            }
        }
        public void ForceInitialize(ObserverUpdateCache observerUpdateCache)
        {
            Initialize(observerUpdateCache);
        }
        protected bool UpdateRenderThread()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // at 144fps timestep would be 0.0069
            if (newTime - ObserverUpdateCache.UpdateTickTimeRenderThread >= 0.0069)
            {
                ObserverUpdateCache.UpdateTickTimeRenderThreadDeltaTimeSec = (newTime - ObserverUpdateCache.UpdateTickTimeRenderThread) * PartialMathUtil.SECOND_PER_MILLISECOND;
                ObserverUpdateCache.UpdateTickTimeRenderThread = newTime;
                return true;
            }
            return false;
        }
        protected void SetObserverUpdateCache()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ObserverUpdateCache = new ObserverUpdateCache(newTime);
        }

        protected void TryAddParticleSystem(GameObject instance)
        {
            ParticleSystem particleSystem = instance.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = instance.AddComponent<ParticleSystem>();
                ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
                emissionModule.enabled = false;

                ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
                shapeModule.enabled = false;
            }
            particleSystem.Stop();
            particleSystem.Simulate(0.0f, true, true);
            particleSystem.Play();
        }
        protected void WarnFixedUpdateTimeChanged()
        {
            if (WarnIncrement++ % 100 == 0)
            {
                Debug.LogWarning("Warning: Time.FixedDeltaTime manually changed");
            }
        }

        protected Vector3 CreateEditorField(string fieldName, Vector3[] vectors,
            int index)
        {
            Vector3 result;
            if (vectors != null && index < vectors.Length)
            {
                result = vectors[index];
                
            }
            else
            {
                result = Vector3.zero;
            }
            return EditorGUILayout.Vector3Field($"{fieldName}{index}", result);
        }

        protected S CreateEditorField<S>(string fieldName, S[] values,
            int index)
        {
            S result;
            if (values != null && index < values.Length)
            {
                result = values[index];
            }
            else
            {
                result = default(S);
            }
            switch (Type.GetTypeCode(typeof(S))) 
            {
                case TypeCode.Single:
                    if (result is float f)
                    {
                        float editorField = EditorGUILayout.FloatField($"{fieldName}{index}", f);
                        if (editorField is S tEditorField)
                        {
                            return tEditorField;
                        }
                    }
                    break;
            }
            throw new NotImplementedException();
        }

        protected Transform[] GetFirstLevelTransforms(Transform t)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in t)
            {
                children.Add(child);
            }
            return children.ToArray();
        }

        protected Vector3 RotateY_Forward_Editor(float zDistance, float cosYAngle, float sinYAngle)
        {
            float rotatedPositionOffsetX = zDistance * sinYAngle;
            float rotatedPositionOffsetZ = zDistance * cosYAngle;

            return new Vector3(rotatedPositionOffsetX, 0f, rotatedPositionOffsetZ);
        }
    }
}
