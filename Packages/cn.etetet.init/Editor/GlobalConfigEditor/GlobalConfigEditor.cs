using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

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
                
                CodeModeChangeHelper.ChangeToCodeMode(codeMode);
                
                AssetDatabase.Refresh();
                InitHelper.ReGenerateProjectFiles();
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
                AssetDatabase.Refresh();
                InitHelper.ReGenerateProjectFiles();
            }
            
            string sceneName = EditorGUILayout.TextField($"SceneName", globalConfig.SceneName);
            if (sceneName != globalConfig.SceneName)
            {
                globalConfig.SceneName = sceneName;
                EditorResHelper.SaveAssets(globalConfig);
                AssetDatabase.Refresh();
            }
            
            string address = EditorGUILayout.TextField($"Address", globalConfig.Address);
            if (address != globalConfig.Address)
            {
                globalConfig.Address = address;
                EditorResHelper.SaveAssets(globalConfig);
                AssetDatabase.Refresh();
            }
            
        }
    }
}