using System;
using UnityEditor;

namespace ET
{
    [CustomEditor(typeof(GlobalConfig))]
    public class GlobalConfigEditor : Editor
    {
        private CodeMode codeMode;
        private BuildType buildType;

        private void OnEnable()
        {
            GlobalConfig globalConfig = (GlobalConfig)this.target;
            this.codeMode = globalConfig.CodeMode;
            globalConfig.BuildType = EditorUserBuildSettings.development? BuildType.Debug : BuildType.Release;
            this.buildType = globalConfig.BuildType;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GlobalConfig globalConfig = (GlobalConfig)this.target;
            if (this.codeMode != globalConfig.CodeMode)
            {
                this.codeMode = globalConfig.CodeMode;
                this.serializedObject.Update();
                AssemblyTool.RefreshCodeMode(globalConfig.CodeMode);
            }

            if (this.buildType != globalConfig.BuildType)
            {
                this.buildType = globalConfig.BuildType;

                switch (this.buildType)
                {
                    case BuildType.Debug:
                        EditorUserBuildSettings.development = true;
                        break;
                    case BuildType.Release:
                        EditorUserBuildSettings.development = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                BuildHelper.ReGenerateProjectFiles();
            }
        }
    }
}
