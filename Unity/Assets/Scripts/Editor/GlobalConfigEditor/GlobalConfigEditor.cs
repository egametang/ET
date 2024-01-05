using UnityEditor;

namespace ET
{
    [CustomEditor(typeof(GlobalConfig))]
    public class GlobalConfigEditor : Editor
    {
        private CodeMode m_CurCodeMode;
        private BuildType m_CurBuildType;

        private void OnEnable()
        {
            GlobalConfig globalConfig = (GlobalConfig)this.target;
            m_CurCodeMode = globalConfig.CodeMode;
            m_CurBuildType = globalConfig.BuildType;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GlobalConfig globalConfig = (GlobalConfig)this.target;
            if (m_CurCodeMode != globalConfig.CodeMode)
            {
                m_CurCodeMode = globalConfig.CodeMode;
                this.serializedObject.Update();
                AssemblyTool.RefreshCodeMode(globalConfig.CodeMode);
            }

            if (m_CurBuildType != globalConfig.BuildType)
            {
                m_CurBuildType = globalConfig.BuildType;
                BuildHelper.ReGenerateProjectFiles();
            }
        }
    }
}
