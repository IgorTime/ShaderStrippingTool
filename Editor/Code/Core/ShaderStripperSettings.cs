using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ShaderStripping
{
    [CreateAssetMenu(
        fileName = nameof(ShaderStripperSettings),
        menuName = "Outlander/ShaderStripper/" + nameof(ShaderStripperSettings))]
    public class ShaderStripperSettings : ScriptableObject
    {
        private const string PLAYER_TOOLTIP = 
            "If false stripper will ignore Player build stage";
        
        private const string ASSET_BUNDLES_TOOLTIP = 
            "If false stripper will ignore Asset Bundles build stage";
        
        private const string SKIP_TOOLTIP = 
            "If true stripper will ignore all shaders which are not included" +
            " neither in AlwaysIncluded, neither in Player.Log file";

        [Tooltip(ASSET_BUNDLES_TOOLTIP)]
        [SerializeField]
        private bool stripAssetBundles = true;

        [Tooltip(PLAYER_TOOLTIP)]
        [SerializeField]
        private bool stripPlayer = true;

        [Tooltip(SKIP_TOOLTIP)]
        [SerializeField]
        private bool skipNotRegisteredShaders = false;

        [Header("Included Shaders:")]
        [SerializeField] private Shader[] alwaysIncluded = Array.Empty<Shader>();
        [SerializeField] private string[] alwaysIncludedByName = Array.Empty<string>();
        
        [Header("Excluded Shaders:")]
        [SerializeField] private Shader[] alwaysExcluded = Array.Empty<Shader>();
        [SerializeField] private string[] alwaysExcludedByName = Array.Empty<string>();

        [Header("WhiteLists:")]
        [SerializeField]
        private ShaderVariantCollection[] manualHandledList =
            Array.Empty<ShaderVariantCollection>();

        [SerializeField] private TextAsset playerLog = default;

        [Header("Logging:")]
        [SerializeField]
        private bool generateOutputShaderVariantCollection = true;

        [SerializeField] private bool generateReport = true;

        public bool StripAssetBundles => stripAssetBundles;
        public bool StripPlayer => stripPlayer;
        public bool SkipNotRegisteredShaders => skipNotRegisteredShaders;
        public TextAsset PlayerLog => playerLog;
        public ShaderVariantCollection[] ManualCollection => manualHandledList;

        public HashSet<string> AlwaysIncluded =>
            GenerateShaderNameSet(alwaysIncluded, alwaysIncludedByName);

        public HashSet<string> AlwaysExcluded =>
            GenerateShaderNameSet(alwaysExcluded, alwaysExcludedByName);

        public bool GenerateOutputShaderVariantCollection => generateOutputShaderVariantCollection;

        public bool GenerateReport => generateReport;

        private void OnValidate()
        {
            ValidatePlayerLog();
        }

        private void ValidatePlayerLog()
        {
            if (playerLog == null)
            {
                return;
            }

            try
            {
                PlayerLogParser.Parse(playerLog);
                Debug.Log("Player.log is valid");
            }
            catch (Exception e)
            {
                playerLog = null;
                Debug.LogException(e);
                throw;
            }
        }

        private static HashSet<string> GenerateShaderNameSet(Shader[] shaders, string[] names)
        {
            var nameList = names.Where(shaderName => !string.IsNullOrEmpty(shaderName)).ToList();
            nameList.AddRange(shaders.Where(shader => shader != null).Select(x => x.name));
            return new HashSet<string>(nameList);
        }
    }
}