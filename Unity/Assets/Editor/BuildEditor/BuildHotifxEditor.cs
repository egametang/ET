using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
#if !ASYNC && !ILRUNTIME
    public class BuildHotifxEditor
    {
        [RuntimeInitializeOnLoadMethod ( RuntimeInitializeLoadType.BeforeSceneLoad )]
        static void BeforeSceneLoad ()
        {
            string batPath = Application.dataPath + "/../Tools/DevAutoCompile.bat";

            ProcessStartInfo info = new ProcessStartInfo
            {
                    FileName = batPath, WindowStyle = ProcessWindowStyle.Hidden, ErrorDialog = true,
                    UseShellExecute = Application.platform == RuntimePlatform.WindowsEditor, Arguments = $"\"{MSBuildPath}\" \"{HotfixPath}\""
            };

            Process p = Process.Start ( info );
            p.WaitForExit ();

            AssetDatabase.Refresh ();
        }

        private const string MSBuildPath = "D:\\Program Files (x86)\\Microsoft Visual Studio 14.0\\Common7\\Tools\\VsDevCmd.bat";

        private const string HotfixPath = "D:\\Projects\\Unity Project\\TDGo\\Client\\Trunk\\Client\\Hotfix\\Unity.Hotfix.csproj";
    }
#endif
}