using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ShaderStripping
{
    public class ShaderVariantCollectionsWhiteList : IShaderWhiteList
    {
        private readonly ShaderVariantCollection[] collections;
        private readonly HashSet<string> registeredShaders;

        public ShaderVariantCollectionsWhiteList(params ShaderVariantCollection[] collections)
        {
            this.collections = collections;

            registeredShaders = new HashSet<string>();
            foreach (var collection in collections)
            {
                ParseShaderVariantCollectionAsset(collection, registeredShaders);
            }
        }

        public bool IsPassed(
            Shader shader,
            in ShaderPass pass,
            params string[] keywords)
        {
            try
            {
                if (collections.Length == 0)
                {
                    return false;
                }

                var variant = new ShaderVariantCollection.ShaderVariant(
                    shader,
                    pass.Type,
                    keywords);

                foreach (var collection in collections)
                {
                    if (collection.Contains(variant))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
            
            return false;
        }

        public bool IsShaderRegistered(string shaderName)
        {
            return registeredShaders.Contains(shaderName);
        }

        private static void ParseShaderVariantCollectionAsset(
            ShaderVariantCollection unityObject,
            HashSet<string> shaderNames)
        {
            var collection = new SerializedObject(unityObject);
            var shadersProperty = collection.FindProperty("m_Shaders");
            for (var i = 0; i < shadersProperty.arraySize; i++)
            {
                var arrayElement = shadersProperty.GetArrayElementAtIndex(i);
                while (arrayElement.Next(true))
                {
                    if (arrayElement.name != "first")
                    {
                        continue;
                    }

                    var shader = arrayElement.objectReferenceValue as Shader;
                    if (shader != null)
                    {
                        shaderNames?.Add(shader.name);
                    }

                    break;
                }
            }
        }
    }
}