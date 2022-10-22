using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShaderStripping
{
    public static class PlayerLogParser
    {
        private const string LINE_PREFIX = "Compiled shader: ";
        private const string NO_KEYWORDS = "no keywords";
        private static readonly string[] partNames = { ", pass: ", ", stage: ", ", keywords " };

        public static Dictionary<string, HashSet<ShaderInfo>> Parse(TextAsset playerLog)
        {
            return !playerLog
                ? new Dictionary<string, HashSet<ShaderInfo>>()
                : Parse(playerLog.text);
        }

        public static Dictionary<string, HashSet<ShaderInfo>> Parse(string playerLog)
        {
            if (string.IsNullOrEmpty(playerLog))
            {
                return new Dictionary<string, HashSet<ShaderInfo>>();
            }

            var result = new Dictionary<string, HashSet<ShaderInfo>>();

            using var reader = new StringReader(playerLog);
            var lineNumber = 0;
            while (ReadNextLine(reader, out var line, ref lineNumber))
            {
                try
                {
                    if (!TryParseLine(line,
                            out var shaderName,
                            out var passName,
                            out var stageName,
                            out var keywords))
                    {
                        continue;
                    }

                    var shaderInfoSet = FindOrCreateSetForShader(result, shaderName);

                    var shaderInfo = new ShaderInfo(
                        shaderName,
                        passName,
                        keywords);

                    shaderInfoSet.Add(shaderInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Can't parse line: {lineNumber}");
                    Debug.LogException(e);
                    throw;
                }
            }

            return result;
        }

        private static HashSet<ShaderInfo> FindOrCreateSetForShader(
            Dictionary<string, HashSet<ShaderInfo>> result,
            string shaderName)
        {
            if (!result.TryGetValue(shaderName, out var compiledList))
            {
                result.Add(shaderName, compiledList = new HashSet<ShaderInfo>());
            }

            return compiledList;
        }

        private static bool ReadNextLine(TextReader reader, out string line, ref int lineNumber)
        {
            lineNumber++;
            line = reader.ReadLine();
            return line != null;
        }

        private static bool TryParseLine(
            string line,
            out string shaderName,
            out string passName,
            out string stageName,
            out string[] keywords)
        {
            if (!line.StartsWith(LINE_PREFIX))
            {
                shaderName = string.Empty;
                passName = string.Empty;
                stageName = string.Empty;
                keywords = Array.Empty<string>();
                return false;
            }
            
            line = line.Replace(LINE_PREFIX, "");

            var parts = line.Split(partNames, StringSplitOptions.None);
            shaderName = parts[0];
            passName = parts[1];
            stageName = parts[2];
            keywords = SplitKeywords(parts[3]);
            return true;
        }

        private static string[] SplitKeywords(string keywordsString)
        {
            return keywordsString.Contains(NO_KEYWORDS)
                ? Array.Empty<string>()
                : keywordsString.Split(' ');
        }
    }
}