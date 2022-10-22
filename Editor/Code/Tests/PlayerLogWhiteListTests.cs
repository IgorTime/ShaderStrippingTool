using NUnit.Framework;
using ShaderStripping;
using UnityEngine;

public class PlayerLogWhiteListTests
{
    private const string PLAYER_LOG =
        "Compiled shader: Custom/HealthChangeBar, pass: Default, stage: all, keywords INVERTED_ADDITIONAL_TEXTURE\n" +
        "Compiled shader: CutoutDiffuse_Simple, pass: FORWARD, stage: all, keywords DIRECTIONAL LIGHTPROBE_SH\n" +
        "Compiled shader: Hidden/Internal-Halo, pass: <Unnamed Pass 0>, stage: all, keywords FOG_EXP\n" +
        "Compiled shader: Hidden/InternalClear, pass: <Unnamed Pass 6>, stage: all, keywords no keywords\n" +
        "Compiled shader: Hidden/InternalClear, pass: <Unnamed Pass 7>, stage: all, keywords no keywords\n" +
        "Compiled shader: Oleg/Caves, pass: ShadowCaster, stage: all, keywords SHADOWS_DEPTH\n" +
        "Compiled shader: Oleg/Cutout_VertexColor_2Sided_Simple, pass: FORWARD, stage: all, keywords DIRECTIONAL FOG_EXP LIGHTPROBE_SH\n" +
        "Compiled shader: Oleg/Cutout_VertexColor_2Sided_Simple, pass: FORWARD, stage: all, keywords DIRECTIONAL FOG_EXP2 INSTANCING_ON LIGHTPROBE_SH\n" +
        "Compiled shader: Oleg/Cutout_VertexColor_2Sided_Simple, pass: FORWARD, stage: all, keywords DIRECTIONAL FOG_EXP2 LIGHTPROBE_SH\n";

    private readonly IShaderWhiteList whitelist = new PlayerLogWhiteList(PLAYER_LOG);
    
    [Test]
    public void PlayerLogWhiteListTestsSimplePasses()
    {
        TestCaseTrue(1, "Custom/HealthChangeBar", "Default", "INVERTED_ADDITIONAL_TEXTURE");
        
        TestCaseTrue(2, "CutoutDiffuse_Simple", "forward", "DIRECTIONAL", "LIGHTPROBE_SH");
        
        TestCaseTrue(3, "Hidden/Internal-Halo", "", "FOG_EXP");
        TestCaseTrue(4, "Hidden/Internal-Halo", "unnamed", "FOG_EXP");
        TestCaseTrue(5, "Hidden/Internal-Halo", "Pass 0", "FOG_EXP");
        
        TestCaseTrue(6, "Hidden/InternalClear", "<Unnamed Pass 0>", "");
        TestCaseFalse(7, "Hidden/InternalClear", "<Unnamed Pass 0>", "FOG_EXP");
        
        TestCaseTrue(8, "Oleg/Cutout_VertexColor_2Sided_Simple", "FORWARD", "DIRECTIONAL", "FOG_EXP", "LIGHTPROBE_SH");
        TestCaseTrue(9, "Oleg/Cutout_VertexColor_2Sided_Simple", "FORWARD", "LIGHTPROBE_SH", "FOG_EXP", "DIRECTIONAL");
        TestCaseFalse(10, "Oleg/Cutout_VertexColor_2Sided_Simple", "FORWARD", "LIGHTPROBE_SH", "FOG_EXP2");
    }

    private void TestCaseTrue(
        int caseNumber,
        string shaderName, 
        string passName, 
        params string[] keywords)
    {
        TestCase(caseNumber, true, shaderName, passName, keywords);
    }
    
    private void TestCaseFalse(
        int caseNumber,
        string shaderName, 
        string passName, 
        params string[] keywords)
    {
        TestCase(caseNumber, false, shaderName, passName, keywords);
    }

    private void TestCase(
        int caseNumber, 
        bool isTrue, 
        string shaderName, 
        string passName, 
        string[] keywords)
    {
        var shader = Shader.Find(shaderName);
        var pass = new ShaderPass { Name = passName };
        var passed = whitelist.IsPassed(shader, pass, keywords);
        var message = $"Case: {caseNumber.ToString()}";
        
        if (isTrue)
        {
            Assert.IsTrue(passed, message);
        }
        else
        {
            Assert.IsFalse(passed, message);
        }
    }
}