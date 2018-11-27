using System;
using System.IO;
using ETModel;
using UnityEditor;

namespace ETEditor
{
    [InitializeOnLoad]
    public class Startup
    {
        private const string ScriptAssembliesDir = "Library/ScriptAssemblies";
        private const string CodeDir = "Assets/Res/Code/";
        private const string HotfixDll = "Unity.Hotfix.dll";
        private const string HotfixPdb = "Unity.Hotfix.pdb";

        static Startup()
        {
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
            
#if ILRuntime
            // Copy最新的pdb文件
            string[] pdbDirs = 
            {
                "./Temp/UnityVS_bin/Debug", 
                "./Temp/UnityVS_bin/Release", 
                "./Temp/bin/Debug", 
                "./Temp/bin/Release"
            };

            DateTime dateTime = DateTime.MinValue;
            string newestPdb = "";
            foreach (string pdbDir in pdbDirs)
            {
                string pdbPath = Path.Combine(pdbDir, HotfixPdb);
                if (!File.Exists(pdbPath))
                {
                    continue;
                }
                FileInfo fi = new FileInfo(pdbPath);
                DateTime lastWriteTimeUtc = fi.LastWriteTimeUtc;
                if (lastWriteTimeUtc > dateTime)
                {
                    newestPdb = pdbPath;
                    dateTime = lastWriteTimeUtc;
                }
            }
            
            if (newestPdb != "")
            {
                File.Copy(Path.Combine(newestPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);
                Log.Info($"复制Hotfix.pdb到Res/Code完成");
            }
#else
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixPdb), Path.Combine(CodeDir, "Hotfix.pdb.bytes"), true);
#endif

            Log.Info($"复制Hotfix.dll, Hotfix.pdb到Res/Code完成");
            AssetDatabase.Refresh ();
        }
    }
}