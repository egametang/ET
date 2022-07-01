using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Compilation;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// https://et-framework.cn/d/615/4
    /// </summary>
    public static class BuildAssemblieEditor
    {
        private const string CodeDir = "Assets/Res/Code/";

        private static bool s_CompileHotfixCompleted = false;
        private static bool m_IsContainsNkgEditorOnlySymbolDefine = false;

        private static string assemblyName;
        private static string[] includeAssemblies;
        private static string[] additionalReferences;
        private static CodeOptimization codeOptimization;
        private static string[] backSymbolDefines;

        [MenuItem("Tools/BuildCodeDebug _F5")]
        public static void BuildCodeDebug()
        {
            assemblyName = "Code";
            includeAssemblies = new[]
            {
                "Packages/Codes/Model/",
                "Packages/Codes/ModelView/",
                "Packages/Codes/Hotfix/",
                "Packages/Codes/HotfixView/"
            };
            additionalReferences = Array.Empty<string>();
            codeOptimization = CodeOptimization.Debug;

            BuildAssemblieEditor.BuildMuteAssembly();
        }

        [MenuItem("Tools/BuildCodeRelease _F6")]
        public static void BuildCodeRelease()
        {
            assemblyName = "Code";
            includeAssemblies = new[]
            {
                "Packages/Codes/Model/",
                "Packages/Codes/ModelView/",
                "Packages/Codes/Hotfix/",
                "Packages/Codes/HotfixView/"
            };
            additionalReferences = Array.Empty<string>();
            codeOptimization = CodeOptimization.Release;

            BuildAssemblieEditor.BuildMuteAssembly();
        }

        [MenuItem("Tools/BuildData _F7")]
        public static void BuildData()
        {
            assemblyName = "Data";
            includeAssemblies = new[]
            {
                "Packages/Codes/Model/",
                "Packages/Codes/ModelView/",
            };
            additionalReferences = Array.Empty<string>();
            codeOptimization = CodeOptimization.Debug;

            BuildAssemblieEditor.BuildMuteAssembly();
        }


        [MenuItem("Tools/BuildLogic _F8")]
        public static void BuildLogic()
        {
            assemblyName = "Logic";
            includeAssemblies = new[]
            {
                "Packages/Codes/Hotfix/",
                "Packages/Codes/HotfixView/"
            };
            additionalReferences = Array.Empty<string>();
            codeOptimization = CodeOptimization.Debug;

            BuildAssemblieEditor.BuildMuteAssembly();
        }


        private static void BuildMuteAssembly()
        {
            s_CompileHotfixCompleted = false;

            string[] backDefineSymbolsForGroup =
                PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup)
                    .Split(';');
            List<string> buildTempDefineSymbolsForGroup = new List<string>();

            m_IsContainsNkgEditorOnlySymbolDefine = false;
            foreach (var defineSymbol in backDefineSymbolsForGroup)
            {
                if (defineSymbol == "NKGEditorOnly")
                {
                    m_IsContainsNkgEditorOnlySymbolDefine = true;
                    continue;
                }

                buildTempDefineSymbolsForGroup.Add(defineSymbol);
            }

            if (m_IsContainsNkgEditorOnlySymbolDefine)
            {
                backSymbolDefines = backDefineSymbolsForGroup;
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                    buildTempDefineSymbolsForGroup.ToArray());

                CompilationPipeline.RequestScriptCompilation();
                CompilationPipeline.compilationFinished += OnSymbolHandleCompleted;
            }
            else
            {
                OnSymbolHandleCompleted(null);
            }
        }

        private static void OnSymbolHandleCompleted(object s)
        {
            CompilationPipeline.compilationFinished -= OnSymbolHandleCompleted;

            List<string> scripts = new List<string>();
            for (int i = 0; i < includeAssemblies.Length; i++)
            {
                DirectoryInfo dti = new DirectoryInfo(includeAssemblies[i]);
                FileInfo[] fileInfos = dti.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
                for (int j = 0; j < fileInfos.Length; j++)
                {
                    scripts.Add(fileInfos[j].FullName);
                }
            }

            string dllPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.dll");
            string pdbPath = Path.Combine(Define.BuildOutputDir, $"{assemblyName}.pdb");
            if (Directory.Exists(Define.BuildOutputDir))
            {
                File.Delete(dllPath);
                File.Delete(pdbPath);
            }
            else
                Directory.CreateDirectory(Define.BuildOutputDir);

            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(dllPath, scripts.ToArray());

            //启用UnSafe
            //assemblyBuilder.compilerOptions.AllowUnsafeCode = true;

            BuildTargetGroup buildTargetGroup =
                BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            assemblyBuilder.compilerOptions.CodeOptimization = codeOptimization;
            assemblyBuilder.compilerOptions.ApiCompatibilityLevel =
                PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            // assemblyBuilder.compilerOptions.ApiCompatibilityLevel = ApiCompatibilityLevel.NET_4_6;

            assemblyBuilder.additionalReferences = additionalReferences;

            assemblyBuilder.flags = AssemblyBuilderFlags.None;
            //AssemblyBuilderFlags.None                 正常发布
            //AssemblyBuilderFlags.DevelopmentBuild     开发模式打包
            //AssemblyBuilderFlags.EditorAssembly       编辑器状态
            assemblyBuilder.referencesOptions = ReferencesOptions.UseEngineModules;

            assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;

            assemblyBuilder.buildTargetGroup = buildTargetGroup;

            assemblyBuilder.excludeReferences = new string[] { "Library/ScriptAssemblies/Unity.Editor.dll" };

            assemblyBuilder.buildStarted += delegate(string assemblyPath)
            {
                Debug.LogFormat("build start：" + assemblyPath);
            };

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

                s_CompileHotfixCompleted = true;

                if (m_IsContainsNkgEditorOnlySymbolDefine)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                        backSymbolDefines.ToArray());
                }
            };

            //assemblyBuilder.excludeReferences = new string[] { "Unity.Editor.dll" };

            EditorApplication.update += CheckCompileHotfixCompleted;

            //开始构建
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("build fail：" + assemblyBuilder.assemblyPath);
            }
        }

        private static void CheckCompileHotfixCompleted()
        {
            if (!s_CompileHotfixCompleted)
            {
                EditorUtility.DisplayProgressBar("正在编译热更程序集，请稍等。。。", "Wait...", 1.0f);
                return;
            }
            
            EditorUtility.ClearProgressBar();

            EditorApplication.update -= CheckCompileHotfixCompleted;
            s_CompileHotfixCompleted = false;

            Debug.Log("Compiling finish");

            Directory.CreateDirectory(CodeDir);

            File.Copy(Path.Combine(Define.BuildOutputDir, "Code.dll"), Path.Combine(CodeDir, "Code.dll.bytes"), true);
            File.Copy(Path.Combine(Define.BuildOutputDir, "Code.pdb"), Path.Combine(CodeDir, "Code.pdb.bytes"), true);

            AssetDatabase.Refresh();
            Debug.Log("copy Code.dll to Bundles/Code success!");
        }
    }
}