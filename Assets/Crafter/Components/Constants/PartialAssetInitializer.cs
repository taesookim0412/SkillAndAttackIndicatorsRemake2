using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Constants
{
    public static class PartialAssetInitializer
    {
        public static readonly Dictionary<AnimVerticesTexture, Dictionary<PlayerComponentModel, Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, AnimVerticesTextureItems>>>> AnimVerticesTextures = null;
        public static void InitializeAssets()
        {
            AnimVerticesTexturesConstants.InitializeAnimVerticesTextures();
        }
    }

}
