#if UNITY_EDITOR

using Sirenix.OdinInspector;
using UnityEditor;


namespace YIUIFramework.Editor
{
    [HideReferenceObjectPicker]
    [HideLabel]
    public class UICreateResModule : BaseCreateModule
    {
        [LabelText("新增模块名称")]
        public string Name;

        [GUIColor(0, 1, 0)]
        [Button("创建", 30)]
        private void Create()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            Create(Name);
        }

        public static void Create(string createName)
        {
            if (string.IsNullOrEmpty(createName))
            {
                UnityTipsHelper.ShowError("请设定 名称");
                return;
            }

            createName = NameUtility.ToFirstUpper(createName);

            var basePath          = $"{UIStaticHelper.UIProjectResPath}/{createName}";
            var prefabsPath       = $"{basePath}/{UIStaticHelper.UIPrefabs}";
            var spritesPath       = $"{basePath}/{UIStaticHelper.UISprites}";
            var spritesAtlas1Path = $"{basePath}/{UIStaticHelper.UISprites}/{UIStaticHelper.UISpritesAtlas1}";
            var atlasIgnorePath   = $"{basePath}/{UIStaticHelper.UISprites}/{UIStaticHelper.UIAtlasIgnore}";
            var atlasPath         = $"{basePath}/{UIStaticHelper.UIAtlas}";
            var sourcePath        = $"{basePath}/{UIStaticHelper.UISource}";

            EditorHelper.CreateExistsDirectory(prefabsPath);
            EditorHelper.CreateExistsDirectory(spritesPath);
            EditorHelper.CreateExistsDirectory(spritesAtlas1Path);
            EditorHelper.CreateExistsDirectory(atlasIgnorePath);
            EditorHelper.CreateExistsDirectory(atlasPath);
            EditorHelper.CreateExistsDirectory(sourcePath);

            MenuItemYIUIPanel.CreateYIUIPanelByPath(sourcePath, createName);

            YIUIAutoTool.CloseWindowRefresh();
        }
    }
}
#endif