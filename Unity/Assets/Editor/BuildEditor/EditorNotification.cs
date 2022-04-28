
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ET
{
    [InitializeOnLoad]
    public class EditorNotification : AssetPostprocessor
    {
        private static bool isFocused;
        static EditorNotification()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (isFocused == UnityEditorInternal.InternalEditorUtility.isApplicationActive)
            {
                return;
            }
            isFocused = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
            OnEditorFocus(isFocused);
        }
        /// <summary>
        /// Unity窗口聚焦状态改变回调
        /// </summary>
        /// <param name="focus"></param>
        private static void OnEditorFocus(bool focus)
        {
            if (focus)
            {
                //每次切回来Unity，自动执行脚本编译
                // Debug.Log($"编辑器激活状态:{focus}");
                bool autoBuild = PlayerPrefs.HasKey("AutoBuild");
                if (!autoBuild)
                    return;
                BuildAssemblieEditor.BuildCodeDebug();
            }
        }

        /// <summary>
        /// Asset下文件改变时回调
        /// </summary>
        private void OnPreprocessAsset()
        {
            //Debug.Log("Asset下文件改变时回调");
        }
    }
}

#endif
