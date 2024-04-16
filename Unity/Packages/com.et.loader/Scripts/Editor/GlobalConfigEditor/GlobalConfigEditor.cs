using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YooAsset;

namespace ET
{
    [CustomEditor(typeof(GlobalConfig))]
    public class GlobalConfigEditor : Editor
    {
        private void OnEnable()
        {
            GlobalConfig globalConfig = (GlobalConfig)this.target;
            globalConfig.BuildType = EditorUserBuildSettings.development ? BuildType.Debug : BuildType.Release;
            EditorResHelper.SaveAssets(globalConfig);
        }

        public override void OnInspectorGUI()
        {
            GlobalConfig globalConfig = (GlobalConfig)this.target;
            CodeMode codeMode = (CodeMode)EditorGUILayout.EnumPopup("CodeMode", globalConfig.CodeMode);
            if (codeMode != globalConfig.CodeMode)
            {
                globalConfig.CodeMode = codeMode;
                EditorResHelper.SaveAssets(globalConfig);
                AssemblyTool.DoCompile();
            }

            bool enableDll = EditorGUILayout.Toggle("EnableDll", globalConfig.EnableDll);
            if (enableDll != globalConfig.EnableDll)
            {
                globalConfig.EnableDll = enableDll;
                EditorResHelper.SaveAssets(globalConfig);
            }
            
            BuildType buildType = (BuildType)EditorGUILayout.EnumPopup("BuildType", globalConfig.BuildType);
            if (buildType != globalConfig.BuildType)
            {
                globalConfig.BuildType = buildType;
                EditorUserBuildSettings.development = globalConfig.BuildType switch
                {
                    BuildType.Debug => true,
                    BuildType.Release => false,
                    _ => throw new ArgumentOutOfRangeException()
                };
                EditorResHelper.SaveAssets(globalConfig);
                AssemblyTool.DoCompile();
            }
            
            EPlayMode ePlayMode = (EPlayMode)EditorGUILayout.EnumPopup("EPlayMode", globalConfig.EPlayMode);
            if (ePlayMode != globalConfig.EPlayMode)
            {
                globalConfig.EPlayMode = ePlayMode;
                EditorResHelper.SaveAssets(globalConfig);
            }
            
            string sceneName = EditorGUILayout.TextField($"SceneName", globalConfig.SceneName);
            if (sceneName != globalConfig.SceneName)
            {
                globalConfig.SceneName = sceneName;
                EditorResHelper.SaveAssets(globalConfig);
            }
        }
    }
}