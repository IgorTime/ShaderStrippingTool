using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable PossibleNullReferenceException

namespace ShaderStripping
{
    public enum StrippingOrder
    {
        Main = 0,
        Output = 100
    }

    public class ShaderStripper :
        ScriptableObject,
        IPreprocessShaders
    {
        // To set Settings use editor "default reference" feature
        [SerializeField] private ShaderStripperSettings settings = default;

        private StrippingReport strippingReport;
        private readonly HashSet<string> alwaysIncluded;
        private readonly HashSet<string> alwaysExcluded;
        private readonly bool strippingEnabled;
        private readonly bool skipNotRegistered;
        private readonly IShaderWhiteList[] shaderWhiteLists;

        public int callbackOrder { get; } = (int)StrippingOrder.Main;

        public ShaderStripper()
        {
            if (!settings)
            {
                settings = CreateInstance<ShaderStripperSettings>();
            }

            shaderWhiteLists = new IShaderWhiteList[]
            {
                new PlayerLogWhiteList(settings.PlayerLog),
                new ShaderVariantCollectionsWhiteList(settings.ManualCollection)
            };

            strippingEnabled =
                !PlayerBuildHelper.IsPlayerBuild && settings.StripAssetBundles ||
                PlayerBuildHelper.IsPlayerBuild && settings.StripPlayer;

            alwaysIncluded = settings.AlwaysIncluded;
            alwaysExcluded = settings.AlwaysExcluded;
            skipNotRegistered = settings.SkipNotRegisteredShaders;

            if (settings.GenerateReport)
            {
                EnableReporting();
            }
        }

        public static string[] GetKeywordNames(ShaderKeywordSet keywordSet)
        {
            var keywords = keywordSet.GetShaderKeywords();
            var result = new string[keywords.Length];
            for (var i = 0; i < keywords.Length; i++)
            {
                result[i] = keywords[i].name;
            }

            return result;
        }

        public void OnProcessShader(
            Shader shader,
            ShaderSnippetData snippet,
            IList<ShaderCompilerData> data)
        {
            strippingReport?.RegisterShaderInReport(shader.name);

            if (CheckStrippingEnabled(shader, data) ||
                CheckAlwaysIncludedShaders(shader, data) ||
                CheckAlwaysExcludedShaders(shader, data) ||
                CheckNotRegisteredShaders(shader, data))
            {
                return;
            }

            StripWhiteListWithReport(shader, snippet, data);
        }

        private bool CheckNotRegisteredShaders(Shader shader, IList<ShaderCompilerData> data)
        {
            if (!skipNotRegistered || IsShaderRegistered(shader.name))
            {
                return false;
            }

            strippingReport?.SetPassed(shader.name, true, data.Count);
            return true;
        }

        private bool CheckAlwaysExcludedShaders(Shader shader, IList<ShaderCompilerData> data)
        {
            if (!alwaysExcluded.Contains(shader.name))
            {
                return false;
            }

            strippingReport?.SetPassed(shader.name, false, data.Count);
            data.Clear();
            return true;
        }

        private bool CheckAlwaysIncludedShaders(Shader shader, IList<ShaderCompilerData> data)
        {
            if (!alwaysIncluded.Contains(shader.name))
            {
                return false;
            }

            strippingReport?.SetPassed(shader.name, true, data.Count);
            return true;
        }

        private bool CheckStrippingEnabled(Shader shader, IList<ShaderCompilerData> data)
        {
            if (strippingEnabled)
            {
                return false;
            }

            strippingReport?.SetPassed(shader.name, true, data.Count);
            return true;
        }

        private void EnableReporting()
        {
            strippingReport = new StrippingReport(
                nameof(ShaderStripper),
                PlayerBuildHelper.IsPlayerBuild);

            EditorApplication.update += WriteReport;

            void WriteReport()
            {
                EditorApplication.update -= WriteReport;
                strippingReport.WriteReportOnDisc();
            }
        }

        private void StripWhiteListWithReport(
            Shader shader,
            ShaderSnippetData snippet,
            IList<ShaderCompilerData> data)
        {
            for (var i = data.Count - 1; i >= 0; --i)
            {
                var keywords = GetKeywordNames(data[i].shaderKeywordSet);
                var pass = new ShaderPass { Name = snippet.passName, Type = snippet.passType };
                var isPassed = DoesShaderPassStripping(shader, pass, keywords);

                strippingReport.SetPassed(shader.name, isPassed);

                if (!isPassed)
                {
                    data.RemoveAt(i);
                }
            }
        }

        private bool DoesShaderPassStripping(Shader shader, in ShaderPass pass, string[] keywords)
        {
            foreach (var whiteList in shaderWhiteLists)
            {
                if (whiteList.IsPassed(shader, pass, keywords))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsShaderRegistered(string shaderName)
        {
            foreach (var shaderWhiteList in shaderWhiteLists)
            {
                if (shaderWhiteList.IsShaderRegistered(shaderName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}