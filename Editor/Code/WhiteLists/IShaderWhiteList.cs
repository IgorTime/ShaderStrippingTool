using UnityEngine;

namespace ShaderStripping
{
    public interface IShaderWhiteList
    {
        bool IsPassed(
            Shader shader,
            in ShaderPass pass,
            params string[] keywords);

        bool IsShaderRegistered(string shaderName);
    }
}