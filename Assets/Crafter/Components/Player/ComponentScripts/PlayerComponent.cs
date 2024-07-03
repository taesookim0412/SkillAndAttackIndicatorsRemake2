using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Player.ComponentScripts
{
    public class PlayerComponent : MonoBehaviour
    {
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
        public void SetCloneFX()
        {
            foreach (Material material in Materials) {
                material.color = Color.black;
            }
        }
    }
}
