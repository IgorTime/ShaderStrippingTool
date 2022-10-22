using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ShaderStripping
{
    public class PlayerBuildHelper : 
        IPreprocessBuildWithReport, 
        IPostprocessBuildWithReport
    {
        public static bool IsPlayerBuild { get; private set; } = false;

        public int callbackOrder { get; } = 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            IsPlayerBuild = true;
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            IsPlayerBuild = false;
        }
    }
}