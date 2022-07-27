using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace ET
{
    public static class BuildAssemblieEditor
    {
        private const string CodeDir = "Assets/Bundles/Code/";

        public static void SetAutoBuildCode()
        {
            PlayerPrefs.SetInt("AutoBuild", 1);
            ShowNotification("AutoBuildCode Enabled");
        }
        
        public static void CancelAutoBuildCode()
        {
            PlayerPrefs.DeleteKey("AutoBuild");
            ShowNotification("AutoBuildCode Disabled");
        }

        public static void BuildCode(CodeOptimization codeOptimization, GlobalConfig globalConfig)
        {
            List<string> codes;
            switch (globalConfig.CodeMode)
            {
                case CodeMode.Client:
                    codes = new List<string>()
                    {
                        "../Codes/Generate/Client/",
                        "../Codes/Model/Share/",
                        "../Codes/Hotfix/Share/",
                        "../Codes/Model/Client/",
                        "../Codes/ModelView/Client/",
                        "../Codes/Hotfix/Client/",
                        "../Codes/HotfixView/Client/"
                    };
                    break;
                case CodeMode.Server:
                    codes = new List<string>()
                    {
                        "../Codes/Generate/Server/",
                        "../Codes/Model/Share/",
                        "../Codes/Hotfix/Share/",
                        "../Codes/Model/Server/",
                        "../Codes/Hotfix/Server/",
                        "../Codes/Model/Client/",
                        "../Codes/Hotfix/Client/",
                    };
                    break;
                case CodeMode.ClientServer:
                    codes = new List<string>()
                    {
                        "../Codes/Generate/Server/",
                        "../Codes/Model/Share/",
                        "../Codes/Hotfix/Share/",
                        "../Codes/Model/Client/",
                        "../Codes/ModelView/Client/",
                        "../Codes/Hotfix/Client/",
                        "../Codes/HotfixView/Client/",
                        "../Codes/Model/Server/",
                        "../Codes/Hotfix/Server/",
                    };
                    break;
                default:
                    throw new Exception("not found enum");
            }

            BuildAssemblieEditor.BuildMuteAssembly("Code", codes, Array.Empty<string>(), codeOptimization);

            AfterCompiling();
            
            AssetDatabase.Refresh();
        }
        
        public static void BuildModel(CodeOptimization codeOptimization, GlobalConfig globalConfig)
        {
            List<string> codes;
            
            switch (globalConfig.CodeMode)
            {
                case CodeMode.Client:
                    codes = new List<string>()
                    {
                        "../Codes/Generate/Client/",
                        "../Codes/Model/Share/",
                        "../Codes/Model/Client/",
                        "../Codes/ModelView/Client/",
                    };
                    break;
                case CodeMode.Server:
                    codes = new List<string>()
                    {
                        "../Codes/Generate/Server/",
                        "../Codes/Model/Share/",
                        "../Codes/Model/Server/",
                        "../Codes/Model/Client/",
                    };
                    break;
                case CodeMode.ClientServer:
                    codes = new List<string>()
                    {
                        "../Codes/Model/Share/",
                        "../Codes/Model/Client/",
                        "../Codes/ModelView/Client/",
                        "../Codes/Generate/Server/",
                        "../Codes/Model/Server/",
                    };
                    break;
                default:
                    throw new Exception("not found enum");
            }
            
            BuildAssemblieEditor.BuildMuteAssembly("Model", codes, Array.Empty<string>(), codeOptimization);
        }
        
        
        public static void BuildHotfix(CodeOptimization codeOptimization, GlobalConfig globalConfig)
        {
            string[] logicFiles = Directory.GetFiles(Define.BuildOutputDir, "Hotfix_*");
            foreach (string file in logicFiles)
            {
                File.Delete(file);
            }
            
            int random = RandomHelper.RandomNumber(100000000, 999999999);
            string logicFile = $"Hotfix_{random}";
            
            List<string> codes;
            switch (globalConfig.CodeMode)
            {
                case CodeMode.Client:
                    codes = new List<string>()
                    {
                        "../Codes/Hotfix/Share/",
                        "../Codes/Hotfix/Client/",
                        "../Codes/HotfixView/Client/"
                    };
                    break;
                case CodeMode.Server:
                    codes = new List<string>()
                    {
                        "../Codes/Hotfix/Share/",
                        "../Codes/Hotfix/Server/",
                        "../Codes/Hotfix/Client/",
                    };
                    break;
                case CodeMode.ClientServer:
                    codes = new List<string>()
                    {
                        "../Codes/Hotfix/Share/",
                        "../Codes/Hotfix/Client/",
                        "../Codes/HotfixView/Client/",
                        "../Codes/Hotfix/Server/",
                    };
                    break;
                default:
                    throw new Exception("not found enum");
            }
            
            BuildAssemblieEditor.BuildMuteAssembly(logicFile, codes, new[]{Path.Combine(Define.BuildOutputDir, "Model.dll")}, codeOptimization);
        }

        private static void BuildMuteAssembly(string assemblyName, List<string> CodeDirectorys, string[] additionalReferences, CodeOptimization codeOptimization)
        {
            if (!Directory.Exists(Define.BuildOutputDir))
            {
                Directory.CreateDirectory(Define.BuildOutputDir);
            }
            List<string> scripts = new List<string>();
            for (int i = 0; i < CodeDirectorys.Count; i++)
            {
                DirectoryInfo dti = new DirectoryInfo(CodeDirectorys[i]);
                FileInfo[] fileInfos = dti.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
                for (int j = 0; j < fileInfos.Length; j++)
                {
                    scripts.Add(fileInfos[j].FullName);
                }
            }

            string dllPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll");
            string pdbPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb");
            File.Delete(dllPath);
            File.Delete(pdbPath);

            Directory.CreateDirectory(Define.BuildOutputDir);

            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(dllPath, scripts.ToArray());
            
            //启用UnSafe
            //assemblyBuilder.compilerOptions.AllowUnsafeCode = true;

            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            assemblyBuilder.compilerOptions.CodeOptimization = codeOptimization;
            assemblyBuilder.compilerOptions.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            // assemblyBuilder.compilerOptions.ApiCompatibilityLevel = ApiCompatibilityLevel.NET_4_6;

            assemblyBuilder.additionalReferences = additionalReferences;
            
            assemblyBuilder.flags = AssemblyBuilderFlags.None;
            //AssemblyBuilderFlags.None                 正常发布
            //AssemblyBuilderFlags.DevelopmentBuild     开发模式打包
            //AssemblyBuilderFlags.EditorAssembly       编辑器状态
            assemblyBuilder.referencesOptions = ReferencesOptions.UseEngineModules;

            assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;

            assemblyBuilder.buildTargetGroup = buildTargetGroup;

            assemblyBuilder.buildStarted += delegate(string assemblyPath) { Debug.LogFormat("build start：" + assemblyPath); };

            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                int errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                int warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("Warnings: {0} - Errors: {1}", warningCount, errorCount);

                if (warningCount > 0)
                {
                    Debug.LogFormat("有{0}个Warning!!!", warningCount);
                }

                if (errorCount > 0)
                {
                    for (int i = 0; i < compilerMessages.Length; i++)
                    {
                        if (compilerMessages[i].type == CompilerMessageType.Error)
                        {
                            Debug.LogError(compilerMessages[i].message);
                        }
                    }
                }
            };
            
            //开始构建
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("build fail：" + assemblyBuilder.assemblyPath);
                return;
            }
        }

        private static void AfterCompiling()
        {
            while (EditorApplication.isCompiling)
            {
                Debug.Log("Compiling wait1");
                // 主线程sleep并不影响编译线程
                Thread.Sleep(1000);
                Debug.Log("Compiling wait2");
            }
            
            Debug.Log("Compiling finish");

            Directory.CreateDirectory(CodeDir);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Code.dll"), Path.Combine(CodeDir, "Code.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Code.pdb"), Path.Combine(CodeDir, "Code.pdb.bytes"), true);
            AssetDatabase.Refresh();
            Debug.Log("copy Code.dll to Bundles/Code success!");
            
            // 设置ab包
            AssetImporter assetImporter1 = AssetImporter.GetAtPath("Assets/Bundles/Code/Code.dll.bytes");
            assetImporter1.assetBundleName = "Code.unity3d";
            AssetImporter assetImporter2 = AssetImporter.GetAtPath("Assets/Bundles/Code/Code.pdb.bytes");
            assetImporter2.assetBundleName = "Code.unity3d";
            AssetDatabase.Refresh();
            Debug.Log("set assetbundle success!");
            
            Debug.Log("build success!");
            //反射获取当前Game视图，提示编译完成
            ShowNotification("Build Code Success");
        }

        public static void ShowNotification(string tips)
        {
            var game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            game?.ShowNotification(new GUIContent($"{tips}"));
        }
    }
    
}