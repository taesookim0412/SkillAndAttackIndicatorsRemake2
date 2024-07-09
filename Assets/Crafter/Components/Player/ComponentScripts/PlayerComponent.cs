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
        public SkinnedMeshRenderer[] Meshes;

        [HideInInspector]
        public Material[] Materials;

        private void Awake()
        {
            InitializeMaterials();
        }
        private void InitializeMaterials()
        {
            int materialsCount = 0;
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in Meshes)
            {
                materialsCount += skinnedMeshRenderer.materials.Length;
            }

            Material[] materials = new Material[materialsCount];
            int i = 0;
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in Meshes)
            {
                foreach (Material material in skinnedMeshRenderer.materials)
                {
                    materials[i++] = material;
                }
            }

            Materials = materials;
        }
        public void InitializeClone()
        {
            GameObject.Destroy(ThirdPersonController);
            GameObject.Destroy(CharacterController);
            GameObject.Destroy(StarterAssetsInputs);
            GameObject.Destroy(PlayerInput);
        }
        public void SetCloneFX()
        {
            foreach (Material material in Materials) {
                material.color = Color.black;
            }
        }
    }
}
