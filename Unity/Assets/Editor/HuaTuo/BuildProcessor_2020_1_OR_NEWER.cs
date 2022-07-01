#if UNITY_2020_1_OR_NEWER
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.IO;
using System;
using UnityEditor.UnityLinker;
using System.Reflection;
using UnityEditor.Il2Cpp;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace Huatuo
{
    public class BuildProcessor_2020_1_OR_NEWER : IPreprocessBuildWithReport
#if UNITY_ANDROID
        , IPostGenerateGradleAndroidProject
#else
        , IPostprocessBuildWithReport
#endif
        , IFilterBuildAssemblies, IPostBuildPlayerScriptDLLs, IUnityLinkerProcessor, IIl2CppProcessor
    {

#if !UNITY_IOS
        [InitializeOnLoadMethod]
        private static void Setup()
        {
            ///
            /// unity允许使用UNITY_IL2CPP_PATH环境变量指定il2cpp的位置，因此我们不再直接修改安装位置的il2cpp，
            /// 而是在本地目录
            ///
            var projDir = Path.GetDirectoryName(Application.dataPath);
            var localIl2cppDir = $"{projDir}/HuatuoData/LocalIl2CppData/il2cpp";
            if (!Directory.Exists(localIl2cppDir))
            {
                Debug.LogError($"本地il2cpp目录:{localIl2cppDir} 不存在，请手动执行 {projDir}/HuatuoData 目录下的 init_local_il2cpp_data.bat 或者 init_local_il2cpp_data.sh 文件");
            }
            Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", localIl2cppDir);
        }
#endif

        /// <summary>
        /// 需要在Prefab上挂脚本的热更dll名称列表，不需要挂到Prefab上的脚本可以不放在这里
        /// 但放在这里的dll即使勾选了 AnyPlatform 也会在打包过程中被排除
        /// 
        /// 另外请务必注意： 需要挂脚本的dll的名字最好别改，因为这个列表无法热更（上线后删除或添加某些非挂脚本dll没问题）。
        /// 
        /// 注意：多热更新dll不是必须的！大多数项目完全可以只有HotFix.dll这一个热更新模块,纯粹出于演示才故意设计了两个热更新模块。
        /// 另外，是否热更新跟dll名毫无关系，凡是不打包到主工程的，都可以是热更新dll。
        /// </summary>
        public static List<string> s_monoHotUpdateDllNames = new List<string>()
        {
            "HotFix.dll",
        };

        /// <summary>
        /// 所有热更新dll列表。放到此列表中的dll在打包时OnFilterAssemblies回调中被过滤。
        /// </summary>
        public static List<string> s_allHotUpdateDllNames = s_monoHotUpdateDllNames.Concat(new List<string>
        {
            // 这里放除了s_monoHotUpdateDllNames以外的脚本不需要挂到资源上的dll列表
            "HotFix2.dll",
        }).ToList();

        ///// <summary>
        ///// 需要拷贝的裁剪dll，在裁剪完成后自动拷贝到 Assets/StreamingAssets 目录，这样在打包时即会包含这些dll
        ///// </summary>
        //static List<string> s_copyDllName = new List<string>
        //{
        //    "mscorlib.dll",
        //};


        public int callbackOrder => 0;

        private static void BuildExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {

        }

        public void OnPreprocessBuild(BuildReport report)
        {

        }

        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            // 将热更dll从打包列表中移除
            return assemblies.Where(ass => s_allHotUpdateDllNames.All(dll => !ass.EndsWith(dll, StringComparison.OrdinalIgnoreCase))).ToArray();
        }


        [Serializable]
        public class ScriptingAssemblies
        {
            public List<string> names;
            public List<int> types;
        }

#if UNITY_ANDROID
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // 由于 Android 平台在 OnPostprocessBuild 调用时已经生成完 apk 文件，因此需要提前调用
            AddBackHotFixAssembliesToJson(null, path);
        }
#endif

        public void OnPostprocessBuild(BuildReport report)
        {
#if !UNITY_ANDROID

            AddBackHotFixAssembliesToJson(report, report.summary.outputPath);
#endif
            //var projectProject = Path.GetFullPath(".");
            //foreach(var name in s_copyDllName)
            //{
            //    File.Delete(Path.Combine(projectProject, "Assets", "StreamingAssets", name));
            //}
        }
        
        private void AddBackHotFixAssembliesToJson(BuildReport report, string path)
        {
            /*
             * ScriptingAssemblies.json 文件中记录了所有的dll名称，此列表在游戏启动时自动加载，
             * 不在此列表中的dll在资源反序列化时无法被找到其类型
             * 因此 OnFilterAssemblies 中移除的条目需要再加回来
             */
            string[] jsonFiles = Directory.GetFiles(Path.GetDirectoryName(path), "ScriptingAssemblies.json", SearchOption.AllDirectories);

            if (jsonFiles.Length == 0)
            {
                Debug.LogError("can not find file ScriptingAssemblies.json");
                return;
            }

            foreach (string file in jsonFiles)
            {
                string content = File.ReadAllText(file);
                ScriptingAssemblies scriptingAssemblies = JsonUtility.FromJson<ScriptingAssemblies>(content);
                foreach (string name in s_monoHotUpdateDllNames)
                {
                    if(!scriptingAssemblies.names.Contains(name))
                    {
                        scriptingAssemblies.names.Add(name);
                        scriptingAssemblies.types.Add(16); // user dll type
                    }
                }
                content = JsonUtility.ToJson(scriptingAssemblies);

                File.WriteAllText(file, content);
            }
        }


        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
            var projectProject = Path.GetFullPath(".");
            //foreach (var name in s_copyDllName)
            //{
            //    var dllPath = Path.Combine(projectProject, "Temp", "StagingArea", "Data", "Managed", name);
            //    if (File.Exists(dllPath))
            //    {
            //        File.Copy(dllPath, Path.Combine(projectProject, "Assets", "StreamingAssets", name), true);
            //    }
            //    else
            //    {
            //        Debug.LogWarning($"can not find the strip dll, path = {dllPath}");
            //    }
            //}
        }

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            return String.Empty;
        }

        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            //// 注意，此处使用的环境变量，指定il2cpp目录
            //// 如果要屏蔽或者修改环境变量，需要清理缓存
            //// 缓存路径为 Library/Il2cppBuildCache
            //// 再通过shell脚本安装或更新时，该缓存会自动清理
            //var il2cppPath = Path.Combine(Path.GetFullPath("unity_il2cpp_with_huatuo"), "project_il2cpp", "il2cpp");
            //Debug.Log($"il2cpp path {il2cppPath}");
            //Environment.SetEnvironmentVariable("UNITY_IL2CPP_PATH", il2cppPath);
        }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        public void OnBeforeConvertRun(BuildReport report, Il2CppBuildPipelineData data)
        {
            var projDir = Path.GetDirectoryName(Application.dataPath);
            var dstPath = $"{projDir}/HuatuoData/AssembliesPostIl2CppStrip/{EditorUserBuildSettings.activeBuildTarget}";

            Directory.CreateDirectory(dstPath);

            string srcStripDllPath = projDir + "/" + (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android ?  "Temp/StagingArea/assets/bin/Data/Managed": "Temp/StagingArea/Data/Managed/");

            foreach(var fileFullPath in Directory.GetFiles(srcStripDllPath, "*.dll"))
            {
                var file = Path.GetFileName(fileFullPath);
                Debug.Log($"copy strip dll {fileFullPath} ==> {dstPath}/{file}");
                File.Copy($"{fileFullPath}", $"{dstPath}/{file}", true);
            }
        }


#if   UNITY_IOS
    // hook UnityEditor.BuildCompletionEventsHandler.ReportPostBuildCompletionInfo() ? 因为没有 mac 打包平台因此不清楚
#endif
    }

}
#endif