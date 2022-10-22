using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ShaderStripping
{
    public class StrippingReport
    {
        private const string REPORT_DIRECTORY_NAME = "ShaderStrippingReports";
        
        private readonly string directoryPath;
        private readonly bool isPlayerBuild;
        private readonly Dictionary<string, int> passedShaders;

        private readonly SortedSet<string> processedShaders;
        private readonly string reportName;
        private readonly Dictionary<string, int> strippedShaders;

        public StrippingReport(
            string reportName,
            bool isPlayerBuild)
        {
            this.reportName = reportName;
            this.isPlayerBuild = isPlayerBuild;

            processedShaders = new SortedSet<string>();
            passedShaders = new Dictionary<string, int>();
            strippedShaders = new Dictionary<string, int>();
            directoryPath = Application.dataPath.Replace("Assets", REPORT_DIRECTORY_NAME);
        }

        private void CreateReportFolderIfNotExist()
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
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
            CreateReportFolderIfNotExist();
            var report = GetReportString();
            WriteReport(report);
        }

        private void WriteReport(string report)
        {
            var fileName = GetReportFileName();
            var path = $"{directoryPath}/{fileName}";
            File.WriteAllText(path, report);
            Debug.Log($"Stripping report stored in: {path}");
        }

        private string GetReportString()
        {
            var strBuilder = new StringBuilder();
            FillHeader(strBuilder);
            FillShaderVariants(strBuilder);
            FillSummary(strBuilder);
            return strBuilder.ToString();
        }

        private string GetReportFileName() =>
            isPlayerBuild
                ? "PlayerStrippingReport.txt"
                : "AssetBundlesStrippingReport.txt";

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