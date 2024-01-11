//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

#if UNITY_EDITOR

using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace YIUIFramework.Editor
{
    /// <summary>
    /// YIUI 自动化工具
    /// </summary>
    public class YIUIAutoTool : OdinMenuEditorWindow
    {
        [MenuItem("Tools/YIUI 自动化工具")]
        private static void OpenWindow()
        {
            var window = GetWindow<YIUIAutoTool>();
            window.Show();
        }

        //[MenuItem("Tools/关闭 YIUI 自动化工具")]
        //错误时使用的 面板出现了错误 会导致如何都打不开 就需要先关闭
        private static void CloseWindow()
        {
            GetWindow<YIUIAutoTool>().Close();
        }

        //关闭后刷新资源
        public static void CloseWindowRefresh()
        {
            CloseWindow();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private OdinMenuTree           m_OdinMenuTree;
        private List<BaseTreeMenuItem> m_AllMenuItem = new List<BaseTreeMenuItem>();

        protected override OdinMenuTree BuildMenuTree()
        {
            m_OdinMenuTree                            =  new OdinMenuTree();
            m_OdinMenuTree.Selection.SelectionChanged += OnSelectionChanged;

            m_AllMenuItem.Clear();

            m_AllMenuItem.Add(
                new TreeMenuItem<UIPublishModule>(this, m_OdinMenuTree,
                    UIPublishModule.m_PublishName, EditorIcons.UnityFolderIcon));

            m_AllMenuItem.Add(
                new TreeMenuItem<UIRedDotModule>(this, m_OdinMenuTree,
                    "红点", EditorIcons.AlertCircle));

            m_AllMenuItem.Add(
                new TreeMenuItem<UII2LocalizationModule>(this, m_OdinMenuTree,
                    "多语言", EditorIcons.SpeechBubblesRound));
            
            m_AllMenuItem.Add(
                new TreeMenuItem<UIMacroModule>(this, m_OdinMenuTree,
                    "宏设置", EditorIcons.SettingsCog));

            m_AllMenuItem.Add(
                new TreeMenuItem<UIETScriptModule>(this, m_OdinMenuTree,
                    "ET生成", EditorIcons.SettingsCog));

            m_OdinMenuTree.Add("全局设置", this, EditorIcons.SettingsCog);

            return m_OdinMenuTree;
        }

        private bool        m_FirstInit           = true;
        private StringPrefs m_LastSelectMenuPrefs = new StringPrefs("YIUIAutoTool_LastSelectMenu", null, "全局设置");

        private void OnSelectionChanged(SelectionChangedType obj)
        {
            if (obj != SelectionChangedType.ItemAdded)
            {
                return;
            }

            if (m_FirstInit)
            {
                m_FirstInit = false;

                foreach (var menu in m_OdinMenuTree.MenuItems)
                {
                    if (menu.Name != m_LastSelectMenuPrefs.Value) continue;
                    menu.Select();
                    return;
                }

                return;
            }

            if (m_OdinMenuTree.Selection.SelectedValue is BaseTreeMenuItem menuItem)
            {
                menuItem.SelectionMenu();
            }

            foreach (var menu in m_OdinMenuTree.MenuItems)
            {
                if (!menu.IsSelected) continue;
                m_LastSelectMenuPrefs.Value = menu.Name;
                break;
            }
        }

        public static StringPrefs UserNamePrefs = new StringPrefs("YIUIAutoTool_UserName", null, "YIUI");

        [LabelText("用户名")]
        [Required("请填写用户名")]
        [ShowInInspector]
        private static string m_Author;
        
        public static string Author
        {
            get
            {
                if (string.IsNullOrEmpty(m_Author))
                {
                    m_Author = UserNamePrefs.Value;
                }

                return m_Author;
            }
        }

        [HideLabel]
        [HideReferenceObjectPicker]
        [ShowInInspector]
        private readonly BaseCreateModule m_UIBaseModule = new CreateUIBaseModule();

        [BoxGroup("全局图集设置", centerLabel: true)]
        [ShowInInspector]
        internal UIAtlasModule AtlasModule = new UIAtlasModule();
        
        protected override void Initialize()
        {
            base.Initialize();
            m_Author = UserNamePrefs.Value;
            m_UIBaseModule?.Initialize();
            AtlasModule?.Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UserNamePrefs.Value = Author;
            m_UIBaseModule?.OnDestroy();
            AtlasModule?.OnDestroy();

            foreach (var menuItem in m_AllMenuItem)
            {
                menuItem.OnDestroy();
            }
        }
    }
}
#endif