using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    public static class CodeModeChangeHelper
    {
        private static readonly string[] moduleDirs = { "Packages", "Library/PackageCache" };

        private static readonly string[] scriptDirs = { "Scripts", "CodeMode" };
        
        private static readonly string[] modelDirs = {"Model", "Hotfix", "ModelView", "HotfixView"};

        private static readonly string[] serverDirs = { "Server", "Client", "Share", "ClientServer" };

        private static readonly HashSet<string> v = new()
        {
            "Client/Scripts/Model/Client",
            "Client/Scripts/Model/Share",
            "Client/CodeMode/Model/Client",
            "Client/Scripts/ModelView/Client",
            "Client/Scripts/ModelView/Share",
            "Client/CodeMode/ModelView/Client",
            "Client/Scripts/Hotfix/Client",
            "Client/Scripts/Hotfix/Share",
            "Client/CodeMode/Hotfix/Client",
            "Client/Scripts/HotfixView/Client",
            "Client/Scripts/HotfixView/Share",
            "Client/CodeMode/HotfixView/Client",

            "Server/Scripts/Model/Server",
            "Server/Scripts/Model/Share",
            "Server/CodeMode/Model/Server",
            "Server/Scripts/Hotfix/Server",
            "Server/Scripts/Hotfix/Share",
            "Server/CodeMode/Hotfix/Server",

            "ClientServer/Scripts/Model/Client",
            "ClientServer/Scripts/Model/Server",
            "ClientServer/Scripts/Model/Share",
            "ClientServer/CodeMode/Model/ClientServer",
            "ClientServer/Scripts/ModelView/Client",
            "ClientServer/Scripts/ModelView/Server",
            "ClientServer/Scripts/ModelView/Share",
            "ClientServer/CodeMode/ModelView/ClientServer",
            "ClientServer/Scripts/Hotfix/Client",
            "ClientServer/Scripts/Hotfix/Server",
            "ClientServer/Scripts/Hotfix/Share",
            "ClientServer/CodeMode/Hotfix/ClientServer",
            "ClientServer/Scripts/HotfixView/Client",
            "ClientServer/Scripts/HotfixView/Server",
            "ClientServer/Scripts/HotfixView/Share",
            "ClientServer/CodeMode/HotfixView/ClientServer"
        };
        
        public static void ChangeToCodeMode(CodeMode codeMode)
        {
            foreach (string a in moduleDirs)
            {
                foreach (string moduleDir in Directory.GetDirectories(a, "com.et.*"))
                {
                    foreach (string scriptDir in scriptDirs)
                    {
                        string p = Path.Combine(moduleDir, scriptDir);
                        if (!Directory.Exists(p))
                        {
                            continue;
                        }

                        foreach (string modelDir in modelDirs)
                        {
                            string c = Path.Combine(p, modelDir);
                            if (!Directory.Exists(c))
                            {
                                continue;
                            }

                            foreach (string serverDir in serverDirs)
                            {
                                string e = Path.Combine(c, serverDir);
                                if (!Directory.Exists(e))
                                {
                                    continue;
                                }

                                HandleAssemblyReferenceFile(codeMode, moduleDir, scriptDir, modelDir, serverDir);
                            }
                        }
                    }
                }
            }

            
        }

        private static void HandleAssemblyReferenceFile(CodeMode codeMode, string moduleDir, string scriptDir, string modelDir, string serverDir)
        {
            string path = $"{codeMode}/{scriptDir}/{modelDir}/{serverDir}";
            string filePath = Path.Combine(moduleDir, scriptDir, modelDir, serverDir, "AssemblyReference.asmref");
            DeleteAssemblyReference(filePath);
            if (v.Contains(path))
            {
                CreateAssemblyReference(filePath, modelDir);
            }
        }

        private static void DeleteAssemblyReference(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        
        private static void CreateAssemblyReference(string path, string modelDir)
        {
            File.WriteAllText(path, $"{{ \"reference\": \"ET.{modelDir}\" }}");
        }
    }
}