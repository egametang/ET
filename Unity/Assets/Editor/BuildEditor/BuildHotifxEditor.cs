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
        private const string HotfixMdb = "Unity.Hotfix.dll.mdb";

        static Startup()
        {
            // Copy最新的pdb文件
            string[] pdbDirs = 
            {
                "Temp/UnityVS_bin/Debug", 
                "Temp/UnityVS_bin/Release", 
                "Temp/Debug", 
                "Temp/Release"
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
            
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixDll), Path.Combine(CodeDir, "Hotfix.dll.bytes"), true);
            File.Copy(Path.Combine(ScriptAssembliesDir, HotfixMdb), Path.Combine(CodeDir, "Hotfix.mdb.bytes"), true);

            Log.Info($"复制Hotfix.dll, Hotfix.mdb到Res/Code完成");
            AssetDatabase.Refresh ();
        }
    }
}