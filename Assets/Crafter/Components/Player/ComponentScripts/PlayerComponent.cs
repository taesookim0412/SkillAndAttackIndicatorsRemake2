using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Systems.Observers;
using StarterAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Crafter.Components.Player.ComponentScripts
{
    public class PlayerComponent : MonoBehaviour
    {
        [SerializeField]
        public CharacterController CharacterController;
        [SerializeField]
        public ThirdPersonController ThirdPersonController;
        [SerializeField]
        public StarterAssetsInputs StarterAssetsInputs;
        [SerializeField]
        public PlayerInput PlayerInput;
        [SerializeField]
        public SkinnedMeshRendererContainer[] Meshes;
        [SerializeField]
        public Animator Animator;

        [NonSerialized, HideInInspector]
        public Material[] Materials;

        [NonSerialized, HideInInspector]
        private static int CurrentTimeSecId = Shader.PropertyToID("_CurrentTimeSec");
        [NonSerialized, HideInInspector]
        private static int EndPositionLocalId = Shader.PropertyToID("_EndPositionLocal");
        [NonSerialized, HideInInspector]
        private static int StartTimeSecId = Shader.PropertyToID("_StartTimeSec");
        [NonSerialized, HideInInspector]
        private static int RequiredTimeSecId = Shader.PropertyToID("_RequiredTimeSec");
        [NonSerialized, HideInInspector]
        private static int RequiredTimeSecReciprocalId = Shader.PropertyToID("_RequiredTimeSecReciprocal");
        [NonSerialized, HideInInspector]
        private static int InvertElapsedTimePercentageId = Shader.PropertyToID("_InvertElapsedTimePercentage");

        //[NonSerialized, HideInInspector]
        //public PlayerComponentCloneItems PlayerComponentCloneItems;

        private void InitializeMaterials()
        {
            int materialsCount = 0;
            foreach (SkinnedMeshRendererContainer holder in Meshes)
            {
                materialsCount += holder.SkinnedMeshRenderer.materials.Length;
            }

            Material[] materials = new Material[materialsCount];
            int i = 0;
            foreach (SkinnedMeshRendererContainer holder in Meshes)
            {
                foreach (Material material in holder.SkinnedMeshRenderer.materials)
                {
                    materials[i++] = material;
                }
            }

            Materials = materials;
        }
        public PlayerComponent CreateInactiveTransparentCloneInstance()
        {
            PlayerComponent playerComponentInstance = GameObject.Instantiate(this);
            playerComponentInstance.gameObject.SetActive(false);
            GameObject.Destroy(playerComponentInstance.ThirdPersonController);
            GameObject.Destroy(playerComponentInstance.CharacterController);
            GameObject.Destroy(playerComponentInstance.StarterAssetsInputs);
            GameObject.Destroy(playerComponentInstance.PlayerInput);
            foreach (SkinnedMeshRendererContainer holder in playerComponentInstance.Meshes)
            {
                holder.SkinnedMeshRenderer.materials = holder.TransparentMaterials;
            }

            playerComponentInstance.InitializeMaterials();

            return playerComponentInstance;
        }
        public void OnCloneFXInit()
        {
            //PlayerComponentCloneItems = new PlayerComponentCloneItems(observerUpdateCache);
            // Since Materials is NonSerialized, it must be re-created after the first pool item is created.
            if (Materials == null)
            {
                InitializeMaterials();
            }
        }
        public void SetMaterialVertexTargetPos(Vector3 targetPos, float requiredTime, bool invertElapsedTimePercentage)
        {
            foreach (Material material in Materials)
            {
                float timeSec = Time.time;
                material.SetFloat(CurrentTimeSecId, timeSec);
                material.SetVector(EndPositionLocalId, targetPos);
                
                material.SetFloat(StartTimeSecId, timeSec);

                material.SetFloat(RequiredTimeSecId, requiredTime);

                float requiredTimeSecReciprocal = requiredTime > 0f ? 1f / requiredTime : 0f;
                material.SetFloat(RequiredTimeSecReciprocalId, requiredTimeSecReciprocal);

                material.SetInt(InvertElapsedTimePercentageId, invertElapsedTimePercentage ? 1 : 0);
            }
        }
        public void SetMaterialTime()
        {
            foreach (Material material in Materials)
            {
                material.SetFloat("_CurrentTimeSec", Time.time);
            }
        }
        public void SetCloneFXOpacity(float opacity)
        {
            foreach (Material material in Materials)
            {
                material.color = new Color(1f, 1f, 1f, opacity);
            }
        }
    }

    //public class PlayerComponentCloneItems
    //{
    //    public TimerStructDco_Observer AnimationTimer = new TimerStructDco_Observer(25L);
    //    public bool AnimationTimerCompleted = false;
    //    public bool AnimationTimerSet = false;
    //    public bool AnimationStarted = false;

    //    public PlayerComponentCloneItems(ObserverUpdateCache observerUpdateCache)
    //    {
    //        AnimationTimer.ObserverUpdateCache = observerUpdateCache;
    //    }
    //}
}
