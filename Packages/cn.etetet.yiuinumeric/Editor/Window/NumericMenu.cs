using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace YIUI.Numeric.Editor
{
    public static class NumericMenu
    {
        [MenuItem("ET/Numeric 数值工具/生成+配置", false, 1001)]
        public static void CreateAndConfig()
        {
            var result = new CreateNumeric().Create();
            if (result)
            {
                EditorApplication.ExecuteMenuItem("ET/Excel/ExcelExporter");
            }

            EditorUtility.DisplayDialog("提示", $"生成+配置 数值数据 {(result ? "成功" : "失败")}", "确认");
        }

        [MenuItem("ET/Numeric 数值工具/生成", false, 1002)]
        public static void Create()
        {
            var result = new CreateNumeric().Create();

            EditorUtility.DisplayDialog("提示", $"生成 数值数据 {(result ? "成功" : "失败")}", "确认");
        }

        [MenuItem("ET/Numeric 数值工具/打开 配置表", false, 1003)]
        public static void OpenExcel()
        {
            var path = $"{Application.dataPath}/../Packages/cn.etetet.yiuinumericconfig/Assets/Editor/Luban/Other/NumericType.xlsx";
            try
            {
                Process.Start(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"打开失败 {e.Message}");
            }
        }

        [MenuItem("ET/Numeric 数值工具/文档", false, 1004)]
        public static void OpenDoc()
        {
            Application.OpenURL("https://lib9kmxvq7k.feishu.cn/wiki/GHDOwsmy0iQQMok3gU7cgxbpn7x");
        }

        [MenuItem("ET/Numeric 数值工具/可视化窗口", false, 1005)]
        public static void OpenWindow()
        {
            NumericTools.OpenWindow();
        }

        public static bool CreateConfigPackage()
        {
            var packagesPath = $"{Application.dataPath}/../Packages";
            var template     = $"{packagesPath}/cn.etetet.yiuinumeric/.Template";
            var target       = "cn.etetet.yiuinumericconfig";

            var sourceFolder = $"{template}/{target}";
            var targetFolder = $"{packagesPath}/{target}";
            return CopyFolder.Copy(sourceFolder, targetFolder);
        }

        public static bool ExistsDemoPackage()
        {
            return System.IO.Directory.Exists($"{Application.dataPath}/../Packages/cn.etetet.yiuinumericdemo");
        }

        public static void DeleteDemoPackage()
        {
            System.IO.Directory.Delete($"{Application.dataPath}/../Packages/cn.etetet.yiuinumericdemo", true);
        }

        public static bool CreateDemoPackage()
        {
            var packagesPath = $"{Application.dataPath}/../Packages";
            var template     = $"{packagesPath}/cn.etetet.yiuinumeric/.Template";
            var target       = "cn.etetet.yiuinumericdemo";

            var sourceFolder = $"{template}/{target}";
            var targetFolder = $"{packagesPath}/{target}";
            return CopyFolder.Copy(sourceFolder, targetFolder);
        }
    }
}