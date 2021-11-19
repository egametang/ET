using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace ET
{
    public static class BuildAssemblieEditor
    {
        private const string ScriptAssembliesDir = "Temp/MyAssembly/";
        private const string CodeDir = "Assets/Res/Code/";

        [MenuItem("Tools/BuildDll")]
        public static void BuildDll()
        {
            BuildAssemblieEditor.BuildMuteAssembly("Code", new []
            {
                "Assets/Model/",
                "Assets/ModelView/",
                "Assets/Hotfix/",
                "Assets/HotfixView/"
            });
            AssetDatabase.Refresh();
        }

        private static void CopyDllAndPdb(string FileName)
        {
            Debug.Log("复制Code.dll到Bundles/Code目录");
            Directory.CreateDirectory(CodeDir);
            File.Copy(Path.Combine(ScriptAssembliesDir, FileName + ".dll"), Path.Combine(CodeDir, FileName + ".dll.bytes"), true);
            File.Copy(Path.Combine(ScriptAssembliesDir, FileName + ".pdb"), Path.Combine(CodeDir, FileName + ".pdb.bytes"), true);
        }

        private static void BuildMuteAssembly(string Name, string[] CodeDirectorys)
        {
            List<string> Scripts = new List<string>();
            for (int i = 0; i < CodeDirectorys.Length; i++)
            {
                DirectoryInfo dti = new DirectoryInfo(CodeDirectorys[i]);
                FileInfo[] fileInfos = dti.GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
                for (int j = 0; j < fileInfos.Length; j++)
                {
                    Scripts.Add(fileInfos[j].FullName);
                }
            }

            string outputAssembly = "Temp/MyAssembly/" + Name + ".dll";

            Directory.CreateDirectory("Temp/MyAssembly");

            AssemblyBuilder assemblyBuilder = new AssemblyBuilder(outputAssembly, Scripts.ToArray());

            //启用UnSafe
            assemblyBuilder.compilerOptions.AllowUnsafeCode = true;

            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);

            assemblyBuilder.compilerOptions.ApiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(buildTargetGroup);
            // assemblyBuilder.compilerOptions.ApiCompatibilityLevel = ApiCompatibilityLevel.NET_4_6;

            assemblyBuilder.flags = AssemblyBuilderFlags.None;
            //AssemblyBuilderFlags.None                 正常发布
            //AssemblyBuilderFlags.DevelopmentBuild     开发模式打包
            //AssemblyBuilderFlags.EditorAssembly       编辑器状态

            assemblyBuilder.referencesOptions = ReferencesOptions.UseEngineModules;

            assemblyBuilder.buildTarget = EditorUserBuildSettings.activeBuildTarget;

            assemblyBuilder.buildTargetGroup = buildTargetGroup;

            //添加额外的宏定义
            // assemblyBuilder.additionalDefines = new[]
            // {
            //     ""
            // };

            //需要排除自身的引用
            assemblyBuilder.excludeReferences = new[]
            {
                "Library/ScriptAssemblies/Unity.Model.dll", 
                "Library/ScriptAssemblies/Unity.ModelView.dll",
                "Library/ScriptAssemblies/Unity.Hotfix.dll", 
                "Library/ScriptAssemblies/Unity.HotfixView.dll"
            };

            assemblyBuilder.buildStarted += delegate(string assemblyPath) { Debug.LogFormat("程序集开始构建：" + assemblyPath); };

            assemblyBuilder.buildFinished += delegate(string assemblyPath, CompilerMessage[] compilerMessages)
            {
                int errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error);
                int warningCount = compilerMessages.Count(m => m.type == CompilerMessageType.Warning);

                Debug.LogFormat("程序集构建完成：" + assemblyPath);
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

                    return;
                }
                
                CopyDllAndPdb(Name);
            };

            //开始构建
            if (!assemblyBuilder.Build())
            {
                Debug.LogErrorFormat("构建程序集失败：" + assemblyBuilder.assemblyPath);
            }
        }
    }
}