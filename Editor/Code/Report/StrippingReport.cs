using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ShaderStripping
{
    public class StrippingReport
    {
        private readonly SortedSet<string> processedShaders;
        private readonly Dictionary<string, int> passedShaders;
        private readonly Dictionary<string, int> strippedShaders;
        private readonly bool isPlayerBuild;
        private readonly string reportName;
        private readonly string reportPath;

        public StrippingReport(
            string reportName,
            string reportPath,
            bool isPlayerBuild)
        {
            this.reportName = reportName;
            this.reportPath = reportPath;
            this.isPlayerBuild = isPlayerBuild;

            processedShaders = new SortedSet<string>();
            passedShaders = new Dictionary<string, int>();
            strippedShaders = new Dictionary<string, int>();
        }

        public static void CreateReportFolderIfNotExist(string reportPath)
        {
            if (!reportPath.Contains("Assets"))
            {
                reportPath = $"Assets/{reportPath}";
            }
            
            if (AssetDatabase.IsValidFolder(reportPath))
            {
                return;
            }

            var lastSlashIndex = reportPath.LastIndexOf("/", StringComparison.Ordinal);
            var parentFolder = reportPath.Substring(0, lastSlashIndex);
            var newFolderName = reportPath.Substring(lastSlashIndex + 1);
            AssetDatabase.CreateFolder(parentFolder, newFolderName);
        }

        public void RegisterShaderInReport(string shaderName)
        {
            var isNew = processedShaders.Add(shaderName);
            if (!isNew)
            {
                return;
            }

            passedShaders[shaderName] = 0;
            strippedShaders[shaderName] = 0;
        }

        public void SetPassed(string shaderName, bool isPassed, int increment = 1)
        {
            var storage = isPassed ? passedShaders : strippedShaders;
            storage[shaderName] += increment;
        }

        public void WriteReportOnDisc()
        {
            CreateReportFolderIfNotExist(reportPath);
            
            var filePath = GetReportFilePath();
            var strBuilder = new StringBuilder();
            FillHeader(strBuilder);
            FillShaderVariants(strBuilder);
            FillSummary(strBuilder);

            var path = $"{Application.dataPath}/{filePath}";
            File.WriteAllText(path, strBuilder.ToString());

            AssetDatabase.Refresh();
        }

        private string GetReportFilePath()
        {
            return isPlayerBuild
                ? $"{reportPath}/PlayerStrippingReport.txt"
                : $"{reportPath}/AssetBundlesStrippingReport.txt";
        }

        private void FillHeader(StringBuilder strBuilder)
        {
            strBuilder.AppendLine(reportName);

            var buildType = isPlayerBuild
                ? "Player build"
                : "AssetBundles build";
            
            strBuilder.AppendLine($"Shaders stripping during {buildType}");
        }

        private void FillShaderVariants(StringBuilder strBuilder)
        {
            foreach (var shader in processedShaders)
            {
                var passedAmount = passedShaders[shader];
                var strippedAmount = strippedShaders[shader];
                var processedAmount = passedAmount + strippedAmount;

                strBuilder.AppendLine();
                strBuilder.Append("Shader: ").Append(shader);
                strBuilder.Append(", Processed variants: : ").Append(processedAmount);
                strBuilder.Append(", Passed variants: ").Append(passedAmount);
                strBuilder.Append(", Stripped variants: ").Append(strippedAmount);
            }
        }

        private void FillSummary(StringBuilder strBuilder)
        {
            var totalPassed = passedShaders.Sum(x => x.Value);
            var totalStripped = strippedShaders.Sum(x => x.Value);
            var totalProcessed = totalPassed + totalStripped;
            var uniqueShadersAmount = processedShaders.Count;

            strBuilder.AppendLine().AppendLine();

            var line = new string('-', 5);
            strBuilder.AppendLine($"{line}Summary{line}");
            strBuilder.AppendLine($"Total shaders: {uniqueShadersAmount.ToString()}");
            strBuilder.AppendLine($"Total processed SV: {totalProcessed.ToString()}");
            strBuilder.AppendLine($"Total passed SV: {totalPassed.ToString()}");
            strBuilder.AppendLine($"Total stripped SV: {totalStripped.ToString()}");
        }
    }
}