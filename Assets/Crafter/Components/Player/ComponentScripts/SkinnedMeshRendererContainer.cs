using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Player.ComponentScripts
{
    public class SkinnedMeshRendererContainer : MonoBehaviour
    {
        [SerializeField]
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        [SerializeField]
        public Material[] TransparentMaterials;
    }
}
