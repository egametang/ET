using HybridCLR.Editor.Installer;
using HybridCLR.Editor.Settings;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_2022 && (UNITY_IOS || UNITY_TVOS || UNITY_VISIONOS)

namespace HybridCLR.Editor.BuildProcessors
{
    public static class AddLil2cppSourceCodeToXcodeproj2022OrNewer
    {

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (!HybridCLRSettings.Instance.enable)
                return;
            string pbxprojFile = BuildProcessorUtil.GetXcodeProjectFile(pathToBuiltProject);
            RemoveExternalLibil2cppOption(pbxprojFile);
            CopyLibil2cppToXcodeProj(pathToBuiltProject);
        }

        private static string TryRemoveDunplicateShellScriptSegment(string pbxprojFile, string pbxprojContent)
        {
            // will appear duplicated Shell Script segment when append to existed xcode project.
            // This is unity bug.
            // we remove duplicated Shell Script to avoid build error.
            string copyFileComment = @"/\* CopyFiles \*/,\s+([A-Z0-9]{24}) /\* ShellScript \*/,\s+([A-Z0-9]{24}) /\* ShellScript \*/,";
            var m = Regex.Match(pbxprojContent, copyFileComment, RegexOptions.Multiline);
            if (!m.Success)
            {
                return pbxprojContent;
            }

            if (m.Groups[1].Value != m.Groups[2].Value)
            {
                throw new BuildFailedException($"find invalid /* ShellScript */ segment");
            }

            int startIndexOfDupShellScript = m.Groups[2].Index;
            int endIndexOfDupShellScript = pbxprojContent.IndexOf(",", startIndexOfDupShellScript);

            pbxprojContent = pbxprojContent.Remove(startIndexOfDupShellScript, endIndexOfDupShellScript + 1 - startIndexOfDupShellScript);
            Debug.LogWarning($"[AddLil2cppSourceCodeToXcodeproj] remove duplicated '/* ShellScript */' from file '{pbxprojFile}'");
            return pbxprojContent;
        }

        private static void RemoveExternalLibil2cppOption(string pbxprojFile)
        {
            string pbxprojContent = File.ReadAllText(pbxprojFile, Encoding.UTF8);
            string removeBuildOption = @"--external-lib-il2-cpp=\""$PROJECT_DIR/Libraries/libil2cpp.a\""";
            if (pbxprojContent.Contains(removeBuildOption))
            {
                pbxprojContent = pbxprojContent.Replace(removeBuildOption, "");
                Debug.Log($"[AddLil2cppSourceCodeToXcodeproj] remove il2cpp build option '{removeBuildOption}' from file '{pbxprojFile}'");
            }
            else
            {
                Debug.LogWarning($"[AddLil2cppSourceCodeToXcodeproj] project.pbxproj remove building option:'{removeBuildOption}' fail. This may occur when 'Append' to existing xcode project in building");
            }

            pbxprojContent = TryRemoveDunplicateShellScriptSegment(pbxprojFile, pbxprojContent);


            File.WriteAllText(pbxprojFile, pbxprojContent, Encoding.UTF8);
        }

        private static void CopyLibil2cppToXcodeProj(string pathToBuiltProject)
        {
            string srcLibil2cppDir = $"{SettingsUtil.LocalIl2CppDir}/libil2cpp";
            string destLibil2cppDir = $"{pathToBuiltProject}/Il2CppOutputProject/IL2CPP/libil2cpp";
            BashUtil.RemoveDir(destLibil2cppDir);
            BashUtil.CopyDir(srcLibil2cppDir, destLibil2cppDir, true);
        }
    }
}
#endif