#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace YIUIFramework.Editor
{
    public class UIPublishModule : BaseYIUIToolModule
    {
        internal const string m_PublishName = "发布";

        [FolderPath]
        [LabelText("所有模块资源路径")]
        [ReadOnly]
        [ShowInInspector]
        private string m_AllPkgPath = UIStaticHelper.UIProjectResPath;

        [BoxGroup("创建模块", centerLabel: true)]
        [ShowInInspector]
        internal UICreateResModule CreateResModule = new UICreateResModule();

        //所有模块
        private List<UIPublishPackageModule> m_AllUIPublishPackageModule = new List<UIPublishPackageModule>();

        private void AddAllPkg()
        {
            EditorHelper.CreateExistsDirectory(m_AllPkgPath);
            var folders = Array.Empty<string>();
            try
            {
                folders = Directory.GetDirectories(EditorHelper.GetProjPath(m_AllPkgPath));
            }
            catch (Exception e)
            {
                Debug.LogError($"获取所有模块错误 请检查 err={e.Message}{e.StackTrace}");
                return;
            }

            foreach (var folder in folders)
            {
                var pkgName   = Path.GetFileName(folder);
                var upperName = NameUtility.ToFirstUpper(pkgName);
                if (upperName != pkgName)
                {
                    Debug.LogError($"这是一个非法的模块[ {pkgName} ]请使用统一方法创建模块 或者满足指定要求");
                    continue;
                }

                var newUIPublishPackageModule = new UIPublishPackageModule(this, pkgName);

                //0 模块
                Tree.AddMenuItemAtPath(m_PublishName,
                    new OdinMenuItem(Tree, pkgName, newUIPublishPackageModule)).AddIcon(EditorIcons.Folder);

                //1 图集
                Tree.AddAllAssetsAtPath($"{m_PublishName}/{pkgName}/{UIStaticHelper.UIAtlasCN}",
                    $"{m_AllPkgPath}/{pkgName}/{UIStaticHelper.UIAtlas}", typeof(SpriteAtlas), true, false);

                //2 预制体
                Tree.AddAllAssetsAtPath($"{m_PublishName}/{pkgName}/{UIStaticHelper.UIPrefabsCN}",
                    $"{m_AllPkgPath}/{pkgName}/{UIStaticHelper.UIPrefabs}", typeof(UIBindCDETable), true, false);

                //3 源文件
                Tree.AddAllAssetsAtPath($"{m_PublishName}/{pkgName}/{UIStaticHelper.UISourceCN}",
                    $"{m_AllPkgPath}/{pkgName}/{UIStaticHelper.UISource}", typeof(UIBindCDETable), true, false);

                //4 精灵
                Tree.AddAllAssetImporterAtPath($"{m_PublishName}/{pkgName}/{UIStaticHelper.UISpritesCN}",
                    $"{m_AllPkgPath}/{pkgName}/{UIStaticHelper.UISprites}", typeof(TextureImporter), true, false);

                m_AllUIPublishPackageModule.Add(newUIPublishPackageModule);
            }
        }

        [GUIColor(0f, 1f, 1f)]
        [Button("UI自动生成绑定替代反射代码", 50)]
        [PropertyOrder(-9999)]
        public static void CreateUIBindProvider()
        {
            new CreateUIBindProviderModule().Create();
        }

        [GUIColor(0.4f, 0.8f, 1)]
        [Button("全部发布", 50)]
        [PropertyOrder(-99)]
        public void PublishAll()
        {
            if (!UIOperationHelper.CheckUIOperation()) return;

            foreach (var module in m_AllUIPublishPackageModule)
            {
                module.PublishCurrent(false); //不要默认重置所有图集设置 有的图集真的会有独立设置
            }

            UnityTipsHelper.CallBackOk("YIUI全部 发布完毕", YIUIAutoTool.CloseWindowRefresh);
        }

        public override void Initialize()
        {
            AddAllPkg();
            CreateResModule?.Initialize();
        }

        public override void OnDestroy()
        {
            CreateResModule?.OnDestroy();
        }
    }
}
#endif