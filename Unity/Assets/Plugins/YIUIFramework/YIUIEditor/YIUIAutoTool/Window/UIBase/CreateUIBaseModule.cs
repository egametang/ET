#if UNITY_EDITOR
using ET.Client;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    /// <summary>
    /// UIBase 模块
    /// </summary>
    [HideReferenceObjectPicker]
    public class CreateUIBaseModule : BaseCreateModule
    {
        [LabelText("YIUI项目命名空间")]
        [ShowInInspector]
        [ReadOnly]
        private const string UINamespace = UIStaticHelper.UINamespace;

        [LabelText("YIUI项目资源路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private const string UIProjectResPath = UIStaticHelper.UIProjectResPath;

        [LabelText("YIUI项目脚本路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private const string UIGenerationPath = UIStaticHelper.UIETComponentGenPath;

        [LabelText("YIUI项目自定义脚本路径")]
        [FolderPath]
        [ShowInInspector]
        [ReadOnly]
        private const string UICodeScriptsPath = UIStaticHelper.UIETSystemGenPath;

        [HideLabel]
        [ShowInInspector]
        private CreateUIBaseEditorData UIBaseData = new CreateUIBaseEditorData();

        public override void Initialize()
        {
        }

        public override void OnDestroy()
        {
        }

        private const string m_CommonPkg = "Common";

        [Button("初始化项目")]
        private void CreateProject()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            EditorHelper.CreateExistsDirectory(UIGenerationPath);
            EditorHelper.CreateExistsDirectory(UIProjectResPath);
            EditorHelper.CreateExistsDirectory(UICodeScriptsPath);
            UICreateResModule.Create(m_CommonPkg); //默认初始化一个common模块
            CopyUIRoot();
            YIUIAutoTool.CloseWindowRefresh();
        }

        private void CopyUIRoot()
        {
            var loadRoot = (GameObject)AssetDatabase.LoadAssetAtPath(UIStaticHelper.UIRootPrefabPath, typeof(Object));
            if (loadRoot == null)
            {
                Debug.LogError($"没有找到原始UIRoot {UIStaticHelper.UIRootPrefabPath}");
                return;
            }

            var newGameObj = Object.Instantiate(loadRoot);
            var commonPath =
                $"{UIProjectResPath}/{m_CommonPkg}/{UIStaticHelper.UIPrefabs}/{YIUIMgrComponent.UIRootName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(newGameObj, commonPath);
            Object.DestroyImmediate(newGameObj);
        }
    }
}
#endif