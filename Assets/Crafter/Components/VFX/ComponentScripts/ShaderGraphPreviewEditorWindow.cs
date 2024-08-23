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
using static UnityEngine.Rendering.DebugUI;

#if UNITY_EDITOR
namespace Assets.Crafter.Components.VFX.ComponentScripts
{
    public class ShaderGraphPreviewEditorWindow : EditorWindow
    {
        private static Dictionary<ShaderType, (string propertyName, ShaderProperty propertyType)[]> ShaderGraphPropertiesDict = new Dictionary<ShaderType, (string propertyName, ShaderProperty propertyType)[]>(1)
        {
            { ShaderType.TrailAdvanced, new (string propertyName, ShaderProperty propertyType)[13] {
                ("_BaseColor", ShaderProperty.Texture2D),
                ("_BaseColorTiling", ShaderProperty.Vector2),
                ("_BaseColorAddSpeed", ShaderProperty.Vector2),
                ("_WindSpeed", ShaderProperty.Vector1),
                ("_DistortionSpeed", ShaderProperty.Vector2),
                ("_Distortion", ShaderProperty.Texture2D),
                ("_Color_01", ShaderProperty.Color),
                ("_Color_02", ShaderProperty.Color),
                ("_ErosionSpeed", ShaderProperty.Vector1),
                ("_Erosion", ShaderProperty.Texture2D),
                ("_ColorMaskSpeed", ShaderProperty.Vector2),
                ("_ColorMask", ShaderProperty.Texture2D),
                ("_ErosionMultiply", ShaderProperty.Vector1)
            } }
        };

        private Shader Shader;
        private Material Material;
        private string ShaderTypeString;

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
            ShaderTypeString = EditorGUILayout.TextField("Shader Type String", ShaderTypeString);

            // Button to apply the changes
            if (GUILayout.Button("Update Shader Graph Property Values"))
            {
                if (Enum.TryParse<ShaderType>(ShaderTypeString, out ShaderType shaderType))
                {
                    UpdateShaderGraphProperties(Shader, Material, shaderType);
                }
                else
                {
                    Debug.LogError("Failed to parse ShaderType.");
                }
            }
        }
        private void UpdateShaderGraphProperties(Shader shader, Material material, ShaderType shaderType)
        {
            (string propertyName, ShaderProperty propertyType)[] shaderGraphProperties = ShaderGraphPropertiesDict[shaderType];

            string path = Path.Combine(Directory.GetCurrentDirectory(), AssetDatabase.GetAssetPath(shader));

            if (!File.Exists(path))
            {
                Debug.LogError("Shader Graph file not found.");
                return;
            }

            // Read the Shader Graph file as text
            string shaderGraphText = File.ReadAllText(path);

            // Read the material properties
            (string propertyName, ShaderProperty propertyType, object value)[] materialProperties = GetMaterialProperties(material, shaderGraphProperties);

            foreach ((string propertyName, ShaderProperty propertyType, object value) in materialProperties)
            {
                Debug.Log($"{propertyName}, {propertyType}, {value}");
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
        }
        private (string propertyName, ShaderProperty propertyType, object value)[] GetMaterialProperties(Material material,
            (string propertyName, ShaderProperty propertyType)[] shaderGraphProperties)
        {
            (string propertyName, ShaderProperty propertyType, object value)[] items = new (string propertyName, ShaderProperty propertyType, object value)[shaderGraphProperties.Length];

            for (int i = 0; i < shaderGraphProperties.Length; i++)
            {
                (string propertyName, ShaderProperty propertyType) = shaderGraphProperties[i];

                object value;
                switch (propertyType)
                {
                    case ShaderProperty.Vector1:
                        value = material.GetFloat(propertyName);
                        break;
                    case ShaderProperty.Vector2:
                        value = material.GetVector(propertyName);
                        break;
                    case ShaderProperty.Color:
                        value = material.GetColor(propertyName);
                        break;
                    case ShaderProperty.Texture2D:
                        value = material.GetTexture(propertyName);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                items[i] = (propertyName, propertyType, value);
            }
            return items;
        }

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
            TrailAdvanced
        }
        internal enum ShaderProperty
        {
            Vector1,
            Vector2,
            Color,
            Texture2D
        }
    }

    
}

#endif