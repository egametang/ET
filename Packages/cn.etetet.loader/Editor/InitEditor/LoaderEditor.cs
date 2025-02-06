using UnityEditor;

namespace ET
{
    public static class LoaderEditor
    {
        [MenuItem("ET/Loader/Init")]
        public static void Init()
        {
#if INITED
            UnityEngine.Debug.LogError("Your project are already inited, if you want to reinit, please remove INITED define in unity!");
            return;
#endif
            
            // 设置GlobalConfig中的SceneName字段
            SceneNameSetHelper.Run();
            
            LinkSlnHelper.Run();
            
            // 刷新4个程序集的asmdef引用
            ScriptsReferencesHelper.Run();
            
            CodeModeChangeHelper.ChangeToCodeMode("ClientServer");
            
            InitScriptHelper.Run();
            
            DefineHelper.EnableDefineSymbols("INITED", true);
        }
    }
}