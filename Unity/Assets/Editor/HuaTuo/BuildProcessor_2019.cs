#if UNITY_2018_1_OR_NEWER && !UNITY_2020_1_OR_NEWER
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
using Huatuo.Editor.GlobalManagers;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif

namespace Huatuo
{
    public class BuildProcessor_2019 : IPreprocessBuildWithReport, IPostprocessBuildWithReport
#if UNITY_ANDROID
        , IPostGenerateGradleAndroidProject
#endif
        , IProcessSceneWithReport, IFilterBuildAssemblies, IPostBuildPlayerScriptDLLs, IUnityLinkerProcessor, IIl2CppProcessor
    {
        /// <summary>
        /// 需要在Prefab上挂脚本的热更dll名称列表，不需要挂到Prefab上的脚本可以不放在这里
        /// 但放在这里的dll即使勾选了 AnyPlatform 也会在打包过程中被排除
        /// 
        /// 另外请务必注意！： 需要挂脚本的dll的名字最好别改，因为这个列表无法热更（上线后删除或添加某些非挂脚本dll没问题）
        /// </summary>
        static List<string> monoDllNames = new List<string>() { "HotFix.dll"};

        static MethodInfo s_BuildReport_AddMessage;

        int IOrderedCallback.callbackOrder => 0;

        static Huatuo_BuildProcessor_2019()
        {
            s_BuildReport_AddMessage = typeof(BuildReport).GetMethod("AddMessage", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog("确认", "建议 Build 之前先打包 AssetBundle\r\n是否继续?", "继续", "取消"))
            {
                s_BuildReport_AddMessage.Invoke(report, new object[] { LogType.Exception, "用户取消", "BuildFailedException" });
                return;
            }
        }

        string[] IFilterBuildAssemblies.OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            // 将热更dll从打包列表中移除
            List<string> newNames = new List<string>(assemblies.Length);

            foreach(string assembly in assemblies)
            {
                bool found = false;
                foreach(string removeName in monoDllNames)
                {
                    if(assembly.EndsWith(removeName, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                    newNames.Add(assembly);
            }
            
            return newNames.ToArray();
        }


        [Serializable]
        public class ScriptingAssemblies
        {
            public List<string> names;
            public List<int> types;
        }

        void IPostprocessBuildWithReport.OnPostprocessBuild(BuildReport report)
        {
            AddBackHotFixAssembliesTo_BinFile(report, null);
        }

#if UNITY_ANDROID
        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
        {
            // 由于 Android 平台在 OnPostprocessBuild 调用时已经生成完 apk 文件，因此需要提前调用
            AddBackHotFixAssembliesTo_BinFile(null, path);
        }
#endif

        private void AddBackHotFixAssembliesTo_BinFile(BuildReport report, string path)
        {
            /*
             * Unity2019 中 dll 加载列表存储在 globalgamemanagers 文件中，此列表在游戏启动时自动加载，
             * 不在此列表中的dll在资源反序列化时无法被找到其类型
             * 因此 OnFilterAssemblies 中移除的条目需要再加回来
             */
#if UNITY_ANDROID
            string[] binFiles = new string[] { "Temp/gradleOut/unityLibrary/src/main/assets/bin/Data/globalgamemanagers" }; // report.files 不包含 Temp/gradleOut 等目录
#else
            // 直接出包和输出vs工程时路径不同，report.summary.outputPath 记录的是前者路径
            string[] binFiles = Directory.GetFiles(Path.GetDirectoryName(report.summary.outputPath), "globalgamemanagers", SearchOption.AllDirectories);
#endif

            if (binFiles.Length == 0)
            {
                Debug.LogError("can not find file ScriptingAssemblies.json");
                return;
            }

            foreach (string binPath in binFiles)
            {
                var binFile = new UnityBinFile();
                binFile.LoadFromFile(binPath);

                ScriptsData scriptsData = binFile.scriptsData;
                foreach (string name in monoDllNames)
                {
                    if(!scriptsData.dllNames.Contains(name))
                    {
                        scriptsData.dllNames.Add(name);
                        scriptsData.dllTypes.Add(16); // user dll type
                    }
                }
                binFile.scriptsData = scriptsData;

                binFile.RebuildAndFlushToFile(binPath);
            }
        }


        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {

        }

        void IPostBuildPlayerScriptDLLs.OnPostBuildPlayerScriptDLLs(BuildReport report)
        {

        }

        string IUnityLinkerProcessor.GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            return String.Empty;
        }

        void IUnityLinkerProcessor.OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        void IUnityLinkerProcessor.OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        void IIl2CppProcessor.OnBeforeConvertRun(BuildReport report, Il2CppBuildPipelineData data)
        {

        }


#if UNITY_IOS
    // hook UnityEditor.BuildCompletionEventsHandler.ReportPostBuildCompletionInfo() ? 因为没有 mac 打包平台因此不清楚
#endif
    }

}
#endif