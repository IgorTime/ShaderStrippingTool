using System.Collections.Generic;
using UnityEngine;

namespace ShaderStripping
{
    public class PlayerLogWhiteList : IShaderWhiteList
    {
        private readonly Dictionary<string, HashSet<ShaderInfo>> compiledShaders;

        public PlayerLogWhiteList(string playerLog)
        {
            compiledShaders = PlayerLogParser.Parse(playerLog);
        }

        public PlayerLogWhiteList(TextAsset playerLog)
        {
            compiledShaders = PlayerLogParser.Parse(playerLog);
        }

        public bool IsPassed(
            Shader shader,
            in ShaderPass pass,
            params string[] keywords)
        {
            var shaderVariant = GetShaderVariantInfo(shader, pass.Name, keywords);
            if (!compiledShaders.TryGetValue(shader.name, out var compiledShaderVariants))
            {
                return false;
            }

            foreach (var compiledShaderVariant in compiledShaderVariants)
            {
                if (ShaderInfo.CustomCompare(shaderVariant, compiledShaderVariant))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsShaderRegistered(string shaderName)
        {
            return compiledShaders.ContainsKey(shaderName);
        }

        private static ShaderInfo GetShaderVariantInfo(
            Shader shader,
            string passName,
            string[] keywords)
        {
            var shaderVariant = new ShaderInfo(
                shader.name,
                passName,
                keywords);

            return shaderVariant;
        }
    }
}