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
        public PlayerComponentCloneItems PlayerComponentCloneItems;

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
        public void OnCloneFXInit(ObserverUpdateCache observerUpdateCache)
        {
            PlayerComponentCloneItems = new PlayerComponentCloneItems(observerUpdateCache);
            SetCloneFXOpacity(0f);
        }
        public void SetCloneFXOpacity(float opacity)
        {
            foreach (Material material in Materials)
            {
                material.color = new Color(1f, 1f, 1f, opacity);
            }
        }
    }

    public class PlayerComponentCloneItems
    {
        public TimerStructDco_Observer AnimationTimer = new TimerStructDco_Observer(25L);
        public bool AnimationTimerCompleted = false;
        public bool AnimationTimerSet = false;
        public bool AnimationStarted = false;

        public PlayerComponentCloneItems(ObserverUpdateCache observerUpdateCache)
        {
            AnimationTimer.ObserverUpdateCache = observerUpdateCache;
        }
    }
}
