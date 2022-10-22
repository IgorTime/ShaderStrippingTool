using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShaderStripping
{
    public readonly struct ShaderInfo : IEquatable<ShaderInfo>
    {
        private const string UNNAMED = "unnamed";
        private static readonly Regex passIndexRegex = new(@"pass [\d]*");

        private readonly string shaderName;
        private readonly string passName;
        private readonly HashSet<string> keywords;
        private readonly bool isUnnamedPass;

        public ShaderInfo(
            string shaderName,
            string passName,
            params string[] keywords)
        {
            this.shaderName = shaderName.ToLower();
            this.passName = passName.ToLower();
            this.keywords = GenerateKeywordsSet(keywords);
            isUnnamedPass = IsUnnamedPass(this.passName);
        }
        
        public ShaderInfo(
            string shaderName,
            string passName,
            IEnumerable<string> keywords)
        {
            this.shaderName = shaderName.ToLower();
            this.passName = passName.ToLower();
            this.keywords = GenerateKeywordsSet(keywords);
            isUnnamedPass = IsUnnamedPass(this.passName);
        }

        public static bool CustomCompare(ShaderInfo a, ShaderInfo b)
        {
            return CompareShaderNames(a, b) &&
                   CompareKeywords(a, b) &&
                   ComparePassNames(a, b);
        }

        private static bool IsUnnamedPass(string passName)
        {
            return string.IsNullOrEmpty(passName) ||
                   passName.Contains(UNNAMED) ||
                   passIndexRegex.IsMatch(passName);
        }

        private static HashSet<string> GenerateKeywordsSet(IEnumerable<string> keywords)
        {
            return new HashSet<string>(keywords.Where(x => !string.IsNullOrEmpty(x))
                                               .Select(x => x.ToLower()));
        }

        private static bool ComparePassNames(ShaderInfo a, ShaderInfo b)
        {
            if (a.isUnnamedPass && b.isUnnamedPass)
            {
                return true;
            }

            return a.passName == b.passName;
        }

        private static bool CompareKeywords(ShaderInfo a, ShaderInfo b)
        {
            if (!a.keywords.SetEquals(b.keywords))
            {
                return false;
            }

            return true;
        }

        private static bool CompareShaderNames(ShaderInfo a, ShaderInfo b)
        {
            if (a.shaderName != b.shaderName)
            {
                return false;
            }

            return true;
        }

        public bool Equals(ShaderInfo other)
        {
            return shaderName == other.shaderName &&
                   passName == other.passName &&
                   keywords.SetEquals(other.keywords);
        }

        public override bool Equals(object obj)
        {
            return obj is ShaderInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = shaderName != null ? shaderName.GetHashCode() : 0;

                hashCode = (hashCode * 397) ^ (passName != null
                    ? passName.GetHashCode()
                    : 0);

                hashCode = (hashCode * 397) ^ (keywords != null
                    ? string.Join("", keywords).GetHashCode()
                    : 0);

                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{shaderName} pass: {passName} keywords: {GetKeywordsString()}";
        }

        private string GetKeywordsString()
        {
            return keywords.Count > 0
                ? string.Join(" ", keywords)
                : "no keywords";
        }
    }
}