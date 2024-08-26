using Assets.Crafter.Components.Models;
using DTT.Utils.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.ShaderGraph;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

#if UNITY_EDITOR
namespace Assets.Crafter.Components.VFX.ComponentScripts
{
    public class ShaderGraphPreviewEditorWindow : EditorWindow
    {
        private static readonly string[] ShaderTypeEnumNames = Enum.GetNames(typeof(ShaderType));
        private static Dictionary<ShaderType, (string propertyName, ShaderPropertyType propertyType)[]> ShaderGraphPropertiesDict = new Dictionary<ShaderType, (string propertyName, ShaderPropertyType propertyType)[]>(1)
        {
            { ShaderType.TrailAdvanced, new (string propertyName, ShaderPropertyType propertyType)[13] {
                ("_BaseColor", ShaderPropertyType.Texture),
                ("_BaseColorTiling", ShaderPropertyType.Vector),
                ("_BaseColorAddSpeed", ShaderPropertyType.Vector),
                ("_WindSpeed", ShaderPropertyType.Float),
                ("_DistortionSpeed", ShaderPropertyType.Vector),
                ("_Distortion", ShaderPropertyType.Texture),
                ("_Color_01", ShaderPropertyType.Color),
                ("_Color_02", ShaderPropertyType.Color),
                ("_ErosionSpeed", ShaderPropertyType.Float),
                ("_Erosion", ShaderPropertyType.Texture),
                ("_ColorMaskSpeed", ShaderPropertyType.Vector),
                ("_ColorMask", ShaderPropertyType.Texture),
                ("_ErosionMultiply", ShaderPropertyType.Float)
            } }
        };

        private Shader Shader;
        private Material Material;
        private ShaderType SelectedShaderType;

        [MenuItem("Tools/Shader Graph Editor")]
        public static void ShowWindow()
        {
            GetWindow<ShaderGraphPreviewEditorWindow>("Shader Graph Preview");
        }

        private void OnGUI()
        {
            // Shader Graph field
            Shader = (Shader)EditorGUILayout.ObjectField("Shader Graph", Shader, typeof(Shader), false);
            Material = (Material)EditorGUILayout.ObjectField("Material", Material, typeof(Material), false);
            SelectedShaderType = (ShaderType)EditorGUILayout.Popup("ShaderType", (int)SelectedShaderType, ShaderTypeEnumNames);

            // Button to apply the changes
            if (GUILayout.Button("Update Shader Graph Property Values"))
            {
                if (SelectedShaderType != ShaderType.Unset)
                {
                    UpdateShaderGraphProperties(Shader, Material, SelectedShaderType);
                }
                else
                {
                    Debug.LogError("Failed to parse ShaderType.");
                }
            }
        }

        private void UpdateShaderGraphProperties(Shader shader, Material material, ShaderType shaderType)
        {
            (string propertyName, ShaderPropertyType propertyType)[] shaderGraphProperties = ShaderGraphPropertiesDict[shaderType];

            string path = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(shader));

            if (!File.Exists(path))
            {
                Debug.LogError("Shader Graph file not found.");
                return;
            }

            // Read the material properties
            (string propertyName, ShaderPropertyType propertyType, object value)[] materialProperties = GetMaterialProperties(material, shaderGraphProperties);

            foreach ((string propertyName, ShaderPropertyType propertyType, object value) in materialProperties)
            {
                Debug.Log($"{propertyName}, {value}");
            }
            // Read the Shader Graph file as text
            string[] shaderGraphText = File.ReadLines(path).ToArray();

            // Step 2: Split the file into individual JSON objects based on unique identifiers
            List<JObject> jObjects = ParseJsonStrings(shaderGraphText);

            foreach ((string propertyName, ShaderPropertyType propertyType, object value) in materialProperties)
            {
                JObject propertyJObject = jObjects.FirstOrDefault(jObject => jObject["m_Name"]?.ToString() == propertyName);

                switch (propertyType)
                {
                    case ShaderPropertyType.Float:
                        propertyJObject["m_Value"] = (float)value;
                        break;
                    case ShaderPropertyType.Vector:
                        propertyJObject["m_Value"] = ((Vector4)value).ToJObject();
                        break;
                    case ShaderPropertyType.Color:
                        propertyJObject["m_Value"] = ((Color)value).ToJObject();
                        break;
                    case ShaderPropertyType.Texture:
                        Texture texture = (Texture)value;
                        if (texture != null)
                        {
                            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(texture, out string guid, out long fileId))
                            {
                                JObject textureJObject = new JObject()
                                {
                                    ["fileID"] = fileId,
                                    ["guid"] = guid,
                                    ["type"] = 3
                                };

                                JObject serializedTextureJObject = new JObject()
                                {
                                    ["texture"] = textureJObject
                                };

                                propertyJObject["m_Value"]["m_SerializedTexture"] = serializedTextureJObject.ToString(Formatting.None);
                            }
                            else
                            {
                                Debug.LogError("Failed to get texture id.");
                            }
                        }
                        else
                        {
                            propertyJObject["m_Value"]["m_SerializedTexture"] = "{\"texture\":{\"instanceID\":0}}";
                        }
                        
                        
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            string[] results = new string[jObjects.Count];
            for (int i = 0; i < jObjects.Count; i++)
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
                    {
                        jsonTextWriter.Formatting = Formatting.Indented;
                        jsonTextWriter.Indentation = 4;
                        jObjects[i].WriteTo(jsonTextWriter);
                        results[i] = stringWriter.ToString();
                    }
                }
            }

            File.WriteAllText(path, string.Join("\n\n", results));
        }
        public List<JObject> ParseJsonStrings(string[] text)
        {
            List<JObject> jsonObjects = new List<JObject>();
            StringBuilder currentObject = new StringBuilder();
            int braceCount = 0;

            foreach (string line in text)
            {
                bool addObject = false;
                foreach (char c in line)
                {
                    if (c == '{')
                    {
                        braceCount++;
                    }
                    else if (c == '}')
                    {
                        if (--braceCount == 0)
                        {
                            addObject = true;
                        }
                    }
                }

                currentObject.AppendLine(line);

                if (addObject)
                {
                    string jObjectString = currentObject.ToString();
                    jsonObjects.Add(JObject.Parse(jObjectString));
                    currentObject.Clear();
                }
            }

            return jsonObjects;
        }
        private (string propertyName, ShaderPropertyType propertyType, object value)[] GetMaterialProperties(Material material,
            (string propertyName, ShaderPropertyType propertyType)[] shaderGraphProperties)
        {
            (string propertyName, ShaderPropertyType propertyType, object value)[] items = new (string propertyName, ShaderPropertyType propertyType, object value)[shaderGraphProperties.Length];

            for (int i = 0; i < shaderGraphProperties.Length; i++)
            {
                (string propertyName, ShaderPropertyType propertyType) = shaderGraphProperties[i];

                object value;
                switch (propertyType)
                {
                    case ShaderPropertyType.Float:
                        value = material.GetFloat(propertyName);
                        break;
                    case ShaderPropertyType.Vector:
                        value = material.GetVector(propertyName);
                        break;
                    case ShaderPropertyType.Color:
                        value = material.GetColor(propertyName);
                        break;
                    case ShaderPropertyType.Texture:
                        value = material.GetTexture(propertyName);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                items[i] = (propertyName, propertyType, value);
            }
            return items;
        }


        // Modify the Shader Graph text
        //foreach (var property in properties)
        //{
        //    var propertyName = property.name;
        //    var propertyValue = property.value;

        //    // Modify the Shader Graph text based on property type
        //    shaderGraphText = ModifyPropertyInShaderGraph(shaderGraphText, propertyName, propertyValue);
        //}

        //// Save the modified Shader Graph file
        //File.WriteAllText(path, shaderGraphText);
        //AssetDatabase.Refresh();

        //private void SetShaderGraphProperty(Shader shaderGraph, string propertyName, float value)
        //{
        //    if (shaderGraph == null)
        //    {
        //        Debug.LogError("Shader Graph is null.");
        //        return;
        //    }

        //    // Get the path to the Shader Graph asset
        //    string path = AssetDatabase.GetAssetPath(shaderGraph);
        //    if (string.IsNullOrEmpty(path))
        //    {
        //        Debug.LogError("Invalid Shader Graph path.");
        //        return;
        //    }

        //    // Load the Shader Graph asset as a serialized object
        //    var shaderGraphObject = new SerializedObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));

        //    // Log all the properties to understand the structure
        //    SerializedProperty iterator = shaderGraphObject.GetIterator();

        //    bool propertyFound = false;
        //    bool stop = false;
        //    while (iterator.NextVisible(true) && !stop)
        //    {
        //        //var relativeType = iterator.FindPropertyRelative("m_Type");
        //        //if (relativeType != null && relativeType.stringValue == "Float") Debug.Log(iterator.floatValue);
        //        if (iterator.name == "m_Name" && iterator.stringValue == propertyName)
        //        {
        //            propertyFound = true;
        //        }
        //        if (propertyFound)
        //        {
        //            if (iterator.name == "m_DefValue[0]")
        //            {
        //                iterator.floatValue = value;
        //                //shaderGraphObject.ApplyModifiedProperties();
        //                //EditorUtility.SetDirty(Shader);
        //                //AssetDatabase.SaveAssets();
        //                shaderGraphObject.ApplyModifiedPropertiesWithoutUndo(); // Try without undo
        //                //AssetDatabase.SaveAssets();
        //                //AssetDatabase.Refresh(); // Refresh to ensure changes are recognized
        //                Debug.Log($"Property '{propertyName}' set to {value} in Shader Graph '{shaderGraph.name}'");
        //                stop = true;
        //                ////SerializedProperty defValue = iterator.FindPropertyRelative("m_DefValue");
        //                //SerializedProperty defValue = iterator.FindPropertyRelative("m_DefValue[0]");
        //                //Debug.Log(defValue);
        //                //if (defValue != null)
        //                //{
        //                //    defValue.floatValue = value;
        //                //    shaderGraphObject.ApplyModifiedProperties();
        //                //    AssetDatabase.SaveAssets();
        //                //    
        //                //    return;
        //                //}
        //            }
        //        }
        //    }

        //    Debug.LogError($"Property '{propertyName}' not found in Shader Graph.");

        //    //// Load the Shader Graph asset as a serialized object
        //    //var graphData = shaderGraphObject.FindProperty("m_GraphData");

        //    //if (graphData == null)
        //    //{
        //    //    Debug.LogError("Failed to find GraphData in the Shader Graph.");
        //    //    return;
        //    //}

        //    //// Access the properties of the Shader Graph
        //    //var properties = graphData.FindPropertyRelative("m_Properties");

        //    //if (properties == null)
        //    //{
        //    //    Debug.LogError("Failed to find properties in the Shader Graph.");
        //    //    return;
        //    //}

        //    //// Find the property by name and change its value
        //    //for (int i = 0; i < properties.arraySize; i++)
        //    //{
        //    //    var property = properties.GetArrayElementAtIndex(i);
        //    //    var propName = property.FindPropertyRelative("m_Name").stringValue;

        //    //    if (propName == propertyName)
        //    //    {
        //    //        var propTypeStr = property.FindPropertyRelative("m_Type").stringValue;

        //    //        // Check if the property is a float type
        //    //        if (propTypeStr == "Float")
        //    //        {
        //    //            property.FindPropertyRelative("m_Value").floatValue = value;
        //    //            shaderGraphObject.ApplyModifiedProperties();
        //    //            AssetDatabase.SaveAssets();
        //    //            Debug.Log($"Property '{propertyName}' set to {value} in Shader Graph '{shaderGraph.name}'");
        //    //            return;
        //    //        }
        //    //        else
        //    //        {
        //    //            Debug.LogError($"Property '{propertyName}' is not a float property.");
        //    //            return;
        //    //        }
        //    //    }
        //    //}

        //    //Debug.LogError($"Property '{propertyName}' not found in the Shader Graph.");
        //}

        internal enum ShaderType
        {
            Unset,
            TrailAdvanced
        }
    }

    
}

#endif