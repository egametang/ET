#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace ET.PackageManager.Editor
{
    /// <summary>
    /// ET包管理 自动化工具
    /// </summary>
    public class ETPackageAutoTool : OdinMenuEditorWindow
    {
        [MenuItem("ET/ETPackage 包管理自动化工具")]
        public static void OpenWindow()
        {
            var window = GetWindow<ETPackageAutoTool>();
            if (window != null)
                window.Show();
        }

        //[MenuItem("ET/关闭ETPackage 包管理自动化工具")]
        //错误时使用的 面板出现了错误 会导致如何都打不开 就需要先关闭
        public static void CloseWindow()
        {
            GetWindow<ETPackageAutoTool>()?.Close();
        }

        //关闭后刷新资源
        public static void CloseWindowRefresh()
        {
            CloseWindow();
            AssetDatabase.SaveAssets();

            //AssetDatabase.Refresh();//下面的刷新更NB
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        public static void UnloadAllAssets()
        {
            PackageHelper.Unload();
            PackageVersionHelper.Unload();
        }

        public static void ReLoadAllAssets()
        {
            PackageHelper.LoadAsset();
            PackageVersionHelper.LoadAsset();
        }

        private OdinMenuTree           m_OdinMenuTree;
        private List<BaseTreeMenuItem> m_AllMenuItem = new List<BaseTreeMenuItem>();

        protected override OdinMenuTree BuildMenuTree()
        {
            m_OdinMenuTree                            =  new OdinMenuTree();
            m_OdinMenuTree.Selection.SelectionChanged += OnSelectionChanged;

            m_AllMenuItem.Clear();

            var assembly = GetAssembly("ET.PackageManager.Editor");
            if (assembly == null) return null;
            Type[] types = assembly.GetTypes();

            var allAutoMenus = new List<ETPackageAutoMenuData>();

            foreach (Type type in types)
            {
                if (type.IsDefined(typeof(ETPackageMenuAttribute), false))
                {
                    ETPackageMenuAttribute attribute = (ETPackageMenuAttribute)Attribute.GetCustomAttribute(type, typeof(ETPackageMenuAttribute));
                    allAutoMenus.Add(new ETPackageAutoMenuData
                                     {
                                         Type     = type,
                                         MenuName = attribute.MenuName,
                                         Order    = attribute.Order
                                     });
                }
            }

            allAutoMenus.Sort((a, b) => a.Order.CompareTo(b.Order));

            foreach (var attribute in allAutoMenus)
            {
                m_AllMenuItem.Add(NewTreeMenuItem(attribute.Type, attribute.MenuName));
            }

            return m_OdinMenuTree;
        }

        public static Assembly GetAssembly(string assemblyName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assemblie in assemblies)
            {
                if (assemblie.GetName().Name == assemblyName)
                {
                    return assemblie;
                }
            }

            Debug.LogError($"没有找到这个程序集 {assemblyName}");
            return null;
        }

        private BaseTreeMenuItem NewTreeMenuItem(Type moduleType, string moduleName)
        {
            var treeMenuItemType = typeof(TreeMenuItem<>);

            var specificTreeMenuItemType = treeMenuItemType.MakeGenericType(moduleType);

            var constructor = specificTreeMenuItemType.GetConstructor(new Type[]
                                                                      {
                                                                          typeof(OdinMenuEditorWindow),
                                                                          typeof(OdinMenuTree),
                                                                          typeof(string)
                                                                      });

            object treeMenuItem = constructor.Invoke(new object[]
                                                     {
                                                         this,
                                                         m_OdinMenuTree,
                                                         moduleName
                                                     });

            return (BaseTreeMenuItem)treeMenuItem;
        }

        private bool        m_FirstInit           = true;
        private StringPrefs m_LastSelectMenuPrefs = new("ETPackageAutoTool_LastSelectMenu");

        private void OnSelectionChanged(SelectionChangedType obj)
        {
            if (obj != SelectionChangedType.ItemAdded)
            {
                return;
            }

            var lastMenuName = m_LastSelectMenuPrefs.Value;

            if (m_FirstInit)
            {
                m_FirstInit = false;

                foreach (var menu in m_OdinMenuTree.MenuItems)
                {
                    if (string.IsNullOrEmpty(lastMenuName))
                    {
                        lastMenuName                = menu.Name;
                        m_LastSelectMenuPrefs.Value = menu.Name;
                    }

                    if (menu.Name != lastMenuName) continue;
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

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var menuItem in m_AllMenuItem)
            {
                menuItem.OnDestroy();
            }
        }
    }
}
#endif
