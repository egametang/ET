using System.Diagnostics;
using System.IO;
using ETModel;
using UnityEditor;

namespace ETEditor
{
    [InitializeOnLoad]
    public class Startup
    {
        private const string ScriptAssembliesDir = "Library/ScriptAssemblies";
        private const string DebugDir = "Temp/UnityVS_bin/Debug";
        private const string CodeDir = "Assets/Res/Code/";
        private const string HotfixDll = "Unity.Hotfix.dll";
        private const string HotfixPdb = "Unity.Hotfix.pdb";
        private const string HotfixMdb = "Unity.Hotfix.dll.mdb";

        static Startup()
        {
            string batPath = $"Tools{Path.DirectorySeparatorChar}pdb2mdb.exe";
            
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = batPath,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = true,
                UseShellExecute = true,
                Arguments = Path.Combine(ScriptAssembliesDir, HotfixDll)
            };
            
            Process p = Process.Start ( info );
            p.WaitForExit ();
            

            if (File.Exists(Path.Combine(DebugDir, HotfixPdb)))
            {
                File.Copy(Path.Combine(DebugDir, HotfixPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);
                Log.Info($"复制Hotfix.pdb到Res/Code完成");
            }
            
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixMdb), Path.Combine(CodeDir, "Hotfix.mdb.bytes"), true);

            Log.Info($"复制Hotfix.dll, Hotfix.mdb到Res/Code完成");
            AssetDatabase.Refresh ();
        }
    }
}