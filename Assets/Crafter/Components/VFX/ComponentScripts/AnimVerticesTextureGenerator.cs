//using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFX;
//using Assets.Crafter.Components.Constants;
//using Assets.Crafter.Components.Editors.Helpers;
//using Assets.Crafter.Components.Models;
//using Assets.Crafter.Components.Player.ComponentScripts;
//using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEditor;
//using UnityEditor.Animations;
//using UnityEngine;


//namespace Assets.Crafter.Components.VFX.ComponentScripts
//{
//#if UNITY_EDITOR
//    public class AnimVerticesTextureGenerator : MonoBehaviour
//    {
//        [SerializeField, HideInInspector]
//        public AnimVerticesTexture AnimVerticesTexture;
//        [SerializeField, HideInInspector]
//        public PlayerComponent PlayerComponent;
//        [SerializeField, HideInInspector]
//        public PlayerMeshSet PlayerMeshSet;
//        [SerializeField, HideInInspector]
//        public Animator Animator;
//        [SerializeField, HideInInspector]
//        public string AnimStateName;
//        [SerializeField, HideInInspector]
//        public int AnimLayerIndex;
//        [SerializeField, HideInInspector]
//        public AnimationClip AnimClip;
//        [SerializeField, HideInInspector]
//        public float AnimClipFrame;
//    }

//    [CustomEditor(typeof(AnimVerticesTextureGenerator))]
//    public class AnimVerticesTextureGeneratorEditor : Editor
//    {
//        public AnimVerticesTextureGenerator Instance;
//        public void OnSceneGUI()
//        {
//            if (Instance.Animator != null)
//            {
//                Instance.Animator.Play(Instance.AnimStateName, Instance.AnimLayerIndex, Instance.AnimClipFrame);
//                Instance.Animator.Update(0.166f);
//            }
//        }
//        public override void OnInspectorGUI()
//        {
//            base.OnInspectorGUI();
//            Instance = (AnimVerticesTextureGenerator)target;

//            Undo.RecordObject(Instance, "All State");

//            Instance.AnimVerticesTexture = (AnimVerticesTexture)EditorGUILayout.Popup((int)Instance.AnimVerticesTexture, AnimVerticesTexturesConstants.AnimVerticesTextureNames);
//            Instance.PlayerMeshSet = (PlayerMeshSet)EditorGUILayout.Popup((int)Instance.PlayerMeshSet, PartialPlayerConstants.PlayerMeshSetNames);

//            EditorGUI.BeginChangeCheck();
//            Instance.PlayerComponent = (PlayerComponent)EditorGUILayout.ObjectField("PlayerComponent", Instance.PlayerComponent, typeof(PlayerComponent), true);
//            Instance.Animator = (Animator)EditorGUILayout.ObjectField("Animator", Instance.Animator, typeof(Animator), true);
//            bool changeAnimator = EditorGUI.EndChangeCheck();

//            if (Instance.Animator != null)
//            {
//                DrawAnimTab(changeAnimator);
//            }
//        }
//        private void DrawAnimTab(bool changeAnimator)
//        {
//            EditorGUI.BeginChangeCheck();
//            Instance.AnimLayerIndex = EditorGUILayout.IntField("AnimLayerIndex", Instance.AnimLayerIndex);
//            Instance.AnimStateName = EditorGUILayout.TextField("State name", Instance.AnimStateName);
//            if (EditorGUI.EndChangeCheck() || changeAnimator)
//            {
//                Instance.AnimClip = PartialEditorHelpers.GetAnimStateClip((AnimatorController)Instance.Animator.runtimeAnimatorController, Instance.AnimLayerIndex, Instance.AnimStateName);
//            }

//            float selectedClipLength;
//            if (Instance.AnimClip != null) 
//            {
//                selectedClipLength = Instance.AnimClip.length;
//            }
//            else
//            {
//                return;
//            }

//            Instance.AnimClipFrame = EditorGUILayout.Slider("AnimClipFrame", Instance.AnimClipFrame, 0f, selectedClipLength);

//            if (GUILayout.Button("Bake Vertex Weighted Positions"))
//            {
//                SkinnedMeshRendererContainer[] smrContainers = Instance.PlayerComponent.Meshes;

//                Dictionary<string, Dictionary<string, AnimVerticesTextureItems>> meshVertexPosTextures = new Dictionary<string, Dictionary<string, AnimVerticesTextureItems>>(smrContainers.Length); 
//                BakeVertexWeightedPositions(
//                    meshVertexPosTextures,
//                    onlyBody: true);
//                Dictionary<string, Dictionary<string, (JObject channelValuesRange, JObject positiveMinChannelValues)>> rangeValuesDict = 
//                    new Dictionary<string, Dictionary<string, (JObject channelValuesRange, JObject positiveMinChannelValues)>>(smrContainers.Length);
//                ConvertRangeValues(meshVertexPosTextures, rangeValuesDict);

//                TrySaveMeshVertexTextures(meshVertexPosTextures, rangeValuesDict);
//            }
//        }
//        private void ConvertRangeValues(Dictionary<string, Dictionary<string, AnimVerticesTextureItems>> meshVertexPosTextures,
//            Dictionary<string, Dictionary<string, (JObject channelValuesRange, JObject positiveMinChannelValues)>> rangeValuesDict)
//        {
//            foreach (KeyValuePair<string, Dictionary<string, AnimVerticesTextureItems>> meshPair in meshVertexPosTextures)
//            {
//                Dictionary<string, (JObject channelValuesRange, JObject positiveMinChannelValues)> submeshRangeValuesDict = 
//                    new Dictionary<string, (JObject channelValuesRange, JObject positiveMinChannelValues)>(meshPair.Value.Count);

//                foreach (KeyValuePair<string, AnimVerticesTextureItems> submeshPair in meshPair.Value)
//                {
//                    submeshRangeValuesDict[submeshPair.Key] = (
//                        channelValuesRange: submeshPair.Value.ChannelValuesRange.ToJObject(),
//                        positiveMinChannelValues: submeshPair.Value.PositiveMinChannelValues.ToJObject());
//                }
//                rangeValuesDict[meshPair.Key] = submeshRangeValuesDict;
//            }
//        }
//        private void BakeVertexWeightedPositions(
//            Dictionary<string, Dictionary<string, AnimVerticesTextureItems>> meshDict,
//            bool onlyBody)
//        {
//            SkinnedMeshRendererContainer[] smrContainers = Instance.PlayerComponent.Meshes;

//            for (int i = 0; i < smrContainers.Length; i++)
//            {
//                SkinnedMeshRendererContainer smrContainer = smrContainers[i];
//                bool skip;
//                if (onlyBody)
//                {
//                    skip = !smrContainer.IsBody;
//                }
//                else
//                {
//                    skip = smrContainer.IsBody;
//                }
//                if (skip)
//                {
//                    continue;
//                }

//                SkinnedMeshRenderer smr = smrContainers[i].SkinnedMeshRenderer;

//                Material[] materials = smr.materials;

//                Dictionary<string, AnimVerticesTextureItems> submeshDict = new Dictionary<string, AnimVerticesTextureItems>(materials.Length);

//                string meshName = smr.name;
//                Mesh mesh = smr.sharedMesh;

//                if (mesh.uv2.Length > 0)
//                {
//                    throw new NotSupportedException("Only one UV map is supported.");
//                }

//                Material firstMaterial = materials[0];
//                Texture baseColorTexture = firstMaterial.mainTexture;
//                int textureWidth = baseColorTexture.width;
//                int textureHeight = baseColorTexture.height;
//                int textureWidthMaxIndex = textureWidth - 1;
//                int textureHeightMaxIndex = textureHeight - 1;
//                float textureWidthFloat = (float)textureWidth;
//                float textureHeightFloat = (float)textureHeight;

//                Vector2[] uvs = mesh.uv;
//                Vector3[] vertices = mesh.vertices;
//                BoneWeight[] boneWeights = mesh.boneWeights;
//                Matrix4x4[] bindPoses = mesh.bindposes;
//                Transform[] bones = smr.bones;

//                // Preprocess bones world matrix * bindPose
//                Matrix4x4[] bonesWorldBindPoseMatrices = new Matrix4x4[bones.Length];
//                for (int j = 0; j < bones.Length; j++) 
//                {
//                    bonesWorldBindPoseMatrices[j] = bones[j].localToWorldMatrix * bindPoses[j];
//                }

//                bool[][] existingUvs = new bool[textureHeight][];
//                for (int height = 0; height < textureHeight; height++)
//                {
//                    existingUvs[height] = new bool[textureWidth];
//                }

//                for (int submeshIndex = 0; submeshIndex < materials.Length; submeshIndex++)
//                {
//                    Vector3[] floatVectors = new Vector3[baseColorTexture.height * baseColorTexture.width];
//                    Vector3 maxChannelValues = Vector3.zero;
//                    Vector3 minChannelValues = Vector3.zero;
//                    int[] triangles = mesh.GetTriangles(submeshIndex);
//                    // Additionally add floatVector to the list so it's simpler and reduces floatVectors indexing.
//                    List<(int yIndex, int xIndex, Vector3 floatVector)> setFloatVectorsIndices = new List<(int yIndex, int xIndex, Vector3 floatVector)>(triangles.Length);

//                    Material material = materials[submeshIndex];

//                    bool first = true;

//                    foreach (int triangleIndex in triangles)
//                    {
//                        Vector3 vertex = vertices[triangleIndex];

//                        Vector3 accumVertexPosition = Vector3.zero;

//                        BoneWeight weight = boneWeights[triangleIndex];

//                        accumVertexPosition += ApplyBoneTransformation(weight.boneIndex0, weight.weight0, vertex, bonesWorldBindPoseMatrices);
//                        accumVertexPosition += ApplyBoneTransformation(weight.boneIndex1, weight.weight1, vertex, bonesWorldBindPoseMatrices);
//                        accumVertexPosition += ApplyBoneTransformation(weight.boneIndex2, weight.weight2, vertex, bonesWorldBindPoseMatrices);
//                        accumVertexPosition += ApplyBoneTransformation(weight.boneIndex3, weight.weight3, vertex, bonesWorldBindPoseMatrices);

//                        // Convert back to local transform.
//                        accumVertexPosition = Instance.transform.InverseTransformPoint(accumVertexPosition);

//                        Vector2 uv = uvs[triangleIndex];

//                        SetVertexValues(floatVectors, textureHeight, textureWidth, 
//                            textureHeightMaxIndex, textureWidthMaxIndex,
//                            uv, existingUvs, setFloatVectorsIndices, accumVertexPosition);

//                        if (!first)
//                        {
//                            if (accumVertexPosition.x > maxChannelValues.x)
//                            {
//                                maxChannelValues.x = accumVertexPosition.x;
//                            }
//                            else if (accumVertexPosition.x < minChannelValues.x)
//                            {
//                                minChannelValues.x = accumVertexPosition.x;
//                            }
//                            if (accumVertexPosition.y > maxChannelValues.y)
//                            {
//                                maxChannelValues.y = accumVertexPosition.y;
//                            }
//                            else if (accumVertexPosition.y < minChannelValues.y)
//                            {
//                                minChannelValues.y = accumVertexPosition.y;
//                            }
//                            if (accumVertexPosition.z > maxChannelValues.z)
//                            {
//                                maxChannelValues.z = accumVertexPosition.z;
//                            }
//                            else if (accumVertexPosition.z < minChannelValues.z)
//                            {
//                                minChannelValues.z = accumVertexPosition.z;
//                            }
//                        }
//                        else
//                        {
//                            minChannelValues = accumVertexPosition;
//                            maxChannelValues = accumVertexPosition;
//                            first = false;
//                        }
//                    }

//                    Vector3 positiveMinChannelValues = -1f * minChannelValues;

//                    Vector3 channelValuesRange = positiveMinChannelValues + maxChannelValues;

//                    Vector3 channelValuesRangeReciprocal = Vector3.zero;
//                    if (channelValuesRange.x > PartialMathUtil.FLOAT_TOLERANCE)
//                    {
//                        channelValuesRangeReciprocal.x = 1f / channelValuesRange.x;
//                    }
//                    if (channelValuesRange.y > PartialMathUtil.FLOAT_TOLERANCE)
//                    {
//                        channelValuesRangeReciprocal.y = 1f / channelValuesRange.y;
//                    }
//                    if (channelValuesRange.z > PartialMathUtil.FLOAT_TOLERANCE)
//                    {
//                        channelValuesRangeReciprocal.z = 1f / channelValuesRange.z;
//                    }

//                    // Ensure the values in floatVectors are between 0f and 1f (they are centered around the origin)
//                    for (int j = 0; j < setFloatVectorsIndices.Count; j++)
//                    {
//                        (int yIndex, int xIndex, Vector3 floatVector) = setFloatVectorsIndices[j];
//                        setFloatVectorsIndices[j] = (yIndex, xIndex, floatVector + positiveMinChannelValues);
//                    }

//                    // Surround the uvs due to precision issues and after all the necessary verts are set.
//                    SurroundExistingUV_5Units(floatVectors, existingUvs, textureHeight, textureWidth, setFloatVectorsIndices);

//                    Texture2D vertexPosTexture = CreateVertexPosTexture(floatVectors, height: textureHeight, width: textureWidth, channelValuesRangeReciprocal: channelValuesRangeReciprocal);
//                    submeshDict[GameObjectUtil.RemoveInstanceName(material.name)] = new AnimVerticesTextureItems(vertexPosTexture, channelValuesRange, positiveMinChannelValues);
//                }
//                meshDict[meshName] = submeshDict;
//            }
//        }

//        private void SurroundExistingUV_5Units(Vector3[] floatVectors, bool[][] existingUvs, int height, int width, List<(int yIndex, int xIndex, Vector3 floatVector)> setFloatVectorsIndices)
//        {
//            int iterOffset = 2;
//            int surroundUnits = 5;
//            foreach ((int setYIndex, int setXIndex, Vector3 floatVector) in setFloatVectorsIndices)
//            {
//                for (int i = 0; i < surroundUnits; i++)
//                {
//                    int yOffset = i - iterOffset;
//                    int newYIndex = setYIndex + yOffset;
//                    if (newYIndex >= 0 && newYIndex < height)
//                    {
//                        bool[] existingUvsRow = existingUvs[newYIndex];
//                        for (int j = 0; j < surroundUnits; j++)
//                        {
//                            int xOffset = j - iterOffset;
//                            int newXIndex = setXIndex + xOffset;
//                            if (newXIndex >= 0 && newXIndex < width && !existingUvsRow[newXIndex])
//                            {
//                                floatVectors[newYIndex * width + newXIndex] = floatVector;
//                                existingUvsRow[newXIndex] = true;
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        private void TrySaveMeshVertexTextures(Dictionary<string, Dictionary<string, AnimVerticesTextureItems>> meshVertexPosTextures,
//            Dictionary<string, Dictionary<string, (JObject channelValuesRange, JObject positiveMinChannelValues)>> rangeValuesDict)
//        {
//            string animVertTextureName = Instance.AnimVerticesTexture.ToString();
//            string prefix = EditorUtility.SaveFilePanelInProject("Save Mesh Vertex Pos Textures And MaxChannelValues", $"{animVertTextureName}__{Instance.name}", "", "", "Assets/Crafter/Components/VFX/AnimVertices/Resources");

//            foreach (KeyValuePair<string, Dictionary<string, AnimVerticesTextureItems>> meshPair in meshVertexPosTextures)
//            {
//                string meshName = meshPair.Key;

//                foreach (KeyValuePair<string, AnimVerticesTextureItems> submeshPair in meshPair.Value) 
//                {
//                    string submeshName = submeshPair.Key;

//                    byte[] bytes = submeshPair.Value.VertexPosTexture.EncodeToPNG();
//                    // separate by double underscore
//                    File.WriteAllBytes($"{prefix}__{meshName}__{submeshName}.png", bytes);
//                }
//            }

//            string rangeValuesJson = JsonConvert.SerializeObject(rangeValuesDict);
//            File.WriteAllText($"{prefix}__{Instance.PlayerMeshSet}__RangeValues.json", rangeValuesJson);

//            AssetDatabase.Refresh();
//        }
//        private Texture2D CreateVertexPosTexture(Vector3[] floatVectors, int height, int width, Vector3 channelValuesRangeReciprocal)
//        {
//            Color[] colors = new Color[floatVectors.Length];

//            for (int i = 0; i < floatVectors.Length; i++) 
//            {
//                Vector3 vector = floatVectors[i];
//                Color color = new Color(vector.x * channelValuesRangeReciprocal.x,
//                    vector.y * channelValuesRangeReciprocal.y,
//                    vector.z * channelValuesRangeReciprocal.z, 
//                    1f);
//                colors[i] = color;
//            }
//            Texture2D texture = new Texture2D(width, height);
//            texture.SetPixels(colors);
//            return texture;
//        }

//        private void SetVertexValues(Vector3[] floatVectors,
//            int height,
//            int width,
//            int heightMaxIndex,
//            int widthMaxIndex,
//            Vector2 uv, bool[][] existingUvs,
//            List<(int yIndex, int xIndex, Vector3 floatVector)> setFloatVectorsIndices, Vector3 accumVertexPosition)
//        {
//            int yIndex = PartialDataTypesUtil.Round(uv.y * heightMaxIndex);
//            int xIndex = PartialDataTypesUtil.Round(uv.x * widthMaxIndex);

//            floatVectors[yIndex * width + xIndex] = accumVertexPosition;
//            existingUvs[yIndex][xIndex] = true;
//            setFloatVectorsIndices.Add((yIndex, xIndex, accumVertexPosition));
//        }
//        //private void SetVertexValues(Vector3[] floatVectors,
//        //    int height,
//        //    int width,
//        //    Vector2 uv, bool[][] existingUvs, Vector3 accumVertexPosition)
//        //{
//        //    int yIndex = PartialDataTypesUtil.Round((1f - uv.y) * height);
//        //    int xIndex = PartialDataTypesUtil.Round(uv.x * width);

//        //    for (int heightOffset = -1; heightOffset < 2; heightOffset++)
//        //    {
//        //        int newHeightIndex = yIndex + heightOffset;
//        //        if (newHeightIndex >= 0 && newHeightIndex < height)
//        //        {
//        //            bool[] rowExistingUvs = existingUvs[newHeightIndex];
//        //            for (int widthOffset = -1; widthOffset < 2; widthOffset++)
//        //            {
//        //                int newWidthIndex = xIndex + widthOffset;
//        //                if (newWidthIndex >= 0 && newWidthIndex < width)
//        //                {
//        //                    if (!rowExistingUvs[newWidthIndex])
//        //                    {
//        //                        floatVectors[newHeightIndex * width + newWidthIndex] = accumVertexPosition;
//        //                        rowExistingUvs[newWidthIndex] = true;
//        //                    }
//        //                }
//        //            }
//        //        }
//        //    }
//        //}

//        private Vector3 ApplyBoneTransformation(int boneIndex, float weight, Vector3 vertex, Matrix4x4[] bonesWorldBindPoseMatrices)
//        {
//            if (weight < PartialMathUtil.FLOAT_TOLERANCE)
//            {
//                return Vector3.zero;
//            }

//            // Transform the vertex by the bone matrix
//            Vector3 transformedVertex = bonesWorldBindPoseMatrices[boneIndex].MultiplyPoint3x4(vertex);

//            // Apply the weight to the transformed vertex
//            return transformedVertex * weight;
//        }
//    }

//#endif
//}

