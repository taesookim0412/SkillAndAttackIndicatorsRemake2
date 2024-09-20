//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Assets.Crafter.Components.Constants
//{
//    public static class AnimVerticesTexturesConstants
//    {
//        public static string[] AnimVerticesTextureNames = Enum.GetNames(typeof(AnimVerticesTexture));
//        public static readonly int AnimVerticesTextureNamesLength = AnimVerticesTextureNames.Length;

//        public static Dictionary<AnimVerticesTexture, Dictionary<PlayerComponentModel, Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, AnimVerticesTextureItems>>>> ANIM_VERTICES_TEXTURES = null;

//        public static void InitializeAnimVerticesTextures()
//        {
//            // capacity is incorrect!
//            Dictionary<AnimVerticesTexture, Dictionary<PlayerComponentModel, Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, AnimVerticesTextureItems>>>> animVerticesTexturesDict =
//                new Dictionary<AnimVerticesTexture, Dictionary<PlayerComponentModel, Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, AnimVerticesTextureItems>>>>(1);

//            // Dash Blink Ability
//            string animVertTexture = AnimVerticesTexture.DashBlinkAbility.ToString();

//            var dashBlinkAbilityDict = new Dictionary<PlayerComponentModel, Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, AnimVerticesTextureItems>>>(1);

//            dashBlinkAbilityDict[PlayerComponentModel.Starter] = LoadPlayerComponentAnimVertTextures(
//                animVertTexture,
//                PlayerComponentModel.Starter.ToString(),
//                PlayerMeshSet.StarterMeshSet,
//                new (PlayerMesh playerMesh, PlayerSubmesh[] playerSubmeshes)[1]
//                {
//                    ( PlayerMesh.Armature_Mesh, new PlayerSubmesh[3]
//                        {
//                            PlayerSubmesh.M_Armature_Arms,
//                            PlayerSubmesh.M_Armature_Body,
//                            PlayerSubmesh.M_Armature_Legs,
//                        })
//                });

//            animVerticesTexturesDict[AnimVerticesTexture.DashBlinkAbility] = dashBlinkAbilityDict;

//            ANIM_VERTICES_TEXTURES = animVerticesTexturesDict;
//        }
//        private static Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, AnimVerticesTextureItems>> LoadPlayerComponentAnimVertTextures(
//            string animVertTexture,
//            string playerComponentModel,
//            PlayerMeshSet playerMeshSet,
//            (PlayerMesh playerMesh, PlayerSubmesh[] playerSubmeshes)[] meshLoads)
//        {
//            TextAsset rangeValuesResource = Resources.Load<TextAsset>(CreateAnimVerticesRangeValuesName(animVertTexture, playerComponentModel, playerMeshSet.ToString()));
//            var rangeValuesDict = JsonConvert.DeserializeObject<Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, (Vector3 channelValuesRange, Vector3 positiveMinChannelValues)>>>(rangeValuesResource.text);

//            var dict = new Dictionary<PlayerMesh, Dictionary<PlayerSubmesh, AnimVerticesTextureItems>>(meshLoads.Length);

//            foreach ((PlayerMesh playerMesh, PlayerSubmesh[] playerSubmeshes) in meshLoads)
//            {
//                string playerMeshString = playerMesh.ToString();
//                Dictionary<PlayerSubmesh, AnimVerticesTextureItems> submeshDict = new Dictionary<PlayerSubmesh, AnimVerticesTextureItems>(playerSubmeshes.Length);
//                foreach (PlayerSubmesh playerSubmesh in playerSubmeshes)
//                {
//                    Texture2D textureAsset = Resources.Load<Texture2D>(CreateAnimVerticesTextureName(animVertTexture, playerComponentModel, playerMeshString, playerSubmesh.ToString()));
//                    (Vector3 channelValuesRange, Vector3 positiveMinChannelValues) = rangeValuesDict[playerMesh][playerSubmesh];

//                    submeshDict[playerSubmesh] = new AnimVerticesTextureItems(textureAsset, channelValuesRange, positiveMinChannelValues);
//                }
//                dict[playerMesh] = submeshDict;
//            }

//            return dict;
//        }

//        private static string CreateAnimVerticesTextureName(string animVertTexture,
//            string playerComponentModel,
//            string meshName,
//            string submeshName)
//        {
//            return $"{animVertTexture}__{playerComponentModel}__{meshName}__{submeshName}";
//        }
//        private static string CreateAnimVerticesRangeValuesName(string animVertTexture,
//            string playerComponentModel, string playerMeshSet)
//        {
//            return $"{animVertTexture}__{playerComponentModel}__{playerMeshSet}__RangeValues";
//        }
//    }
//    public class AnimVerticesTextureItems
//    {
//        public Texture2D VertexPosTexture;
//        public Vector3 ChannelValuesRange;
//        public Vector3 PositiveMinChannelValues;

//        public AnimVerticesTextureItems(Texture2D vertexPosTexture, Vector3 channelValuesRange, Vector3 positiveMinChannelValues)
//        {
//            VertexPosTexture = vertexPosTexture;
//            ChannelValuesRange = channelValuesRange;
//            PositiveMinChannelValues = positiveMinChannelValues;
//        }
//    }
//    public enum AnimVerticesTexture
//    {
//        None, 
//        DashBlinkAbility
//    }
//}
