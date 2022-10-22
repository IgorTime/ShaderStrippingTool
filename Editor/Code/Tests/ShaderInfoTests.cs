using System;
using NUnit.Framework;
using ShaderStripping;

public class ShaderInfoTests
{
    [Test]
    public void TestUnnamedPassesEquality()
    {
        const string SHADER_NAME = "Shader";
        var keywords = new[] { "keyword" };

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, "Pass 0", keywords);
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, "", keywords);
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 1");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, "<Unnamed Pass 0>", keywords);
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, "", keywords);
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 2");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, "<unnamed>", keywords);
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, "", keywords);
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 3");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, "Pass 0", keywords);
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, "unnamed", keywords);
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 4");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, "<Unnamed Pass 12>", keywords);
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, "unnamed", keywords);
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 5");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, "<Unnamed Pass 12>", keywords);
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, "Pass 2", keywords);
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 6");
        }
    }

    [Test]
    public void TestKeywordsEquality()
    {
        const string SHADER_NAME = "Shader";
        const string PASS_NAME = "Pass";

        // TRUE
        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k2", "k1");
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k1", "k2");
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 1");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k2");
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k2");
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 2");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, PASS_NAME, Array.Empty<string>());
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, PASS_NAME, Array.Empty<string>());
            Assert.IsTrue(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 3");
        }


        // FALSE
        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k1", "k3");
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k1", "k2");
            Assert.IsFalse(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 4");
        }

        {
            var shaderInfo1 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k1", "k2");
            var shaderInfo2 = new ShaderInfo(SHADER_NAME, PASS_NAME, "k1", "k2", "k3");
            Assert.IsFalse(ShaderInfo.CustomCompare(shaderInfo1, shaderInfo2), "Case 5");
        }
    }
}