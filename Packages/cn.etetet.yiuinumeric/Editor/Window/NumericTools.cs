using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace YIUI.Numeric.Editor
{
    public partial class NumericTools : EditorWindow
    {
        public static void OpenWindow()
        {
            var window = GetWindow<NumericTools>("数值系统");
            if (window != null)
                window.Show();
        }

        public static void CloseWindow()
        {
            GetWindow<NumericTools>()?.Close();
        }

        public static void CloseWindowRefresh()
        {
            CloseWindow();
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("Assets/Refresh");
        }

        private void OnGUI()
        {
            OnGUITools();
        }

        private void OnGUITools()
        {
            if (GUILayout.Button("Numeric 文档"))
            {
                NumericMenu.OpenDoc();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("生成+配置", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
            {
                NumericMenu.CreateAndConfig();
            }

            if (GUILayout.Button("生成", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            {
                NumericMenu.Create();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("打开 配置表", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
            {
                NumericMenu.OpenExcel();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("初始化 创建配置包", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
            {
                if (NumericMenu.CreateConfigPackage())
                {
                    CloseWindowRefresh();
                }
            }

            GUILayout.Space(10);
            if (NumericMenu.ExistsDemoPackage())
            {
                if (GUILayout.Button("删除 Demo包", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
                {
                    NumericMenu.DeleteDemoPackage();
                    CloseWindowRefresh();
                }
            }
            else
            {
                if (GUILayout.Button("创建 Demo包", GUILayout.ExpandWidth(true), GUILayout.Height(50)))
                {
                    if (NumericMenu.CreateDemoPackage())
                    {
                        CloseWindowRefresh();
                    }
                }
            }
        }
    }
}