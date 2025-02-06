using System;
using UnityEditor;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    //为了防止其他包的按钮可能改名字等操作 这里需要使用前深度检查
    //因为这个管理包 不可能每次其他包更新都去检查一下 万一哪天没检查到不就BUG了
    //所以这哪里必须要做好防护措施
    public static class PackageExecuteMenuItemHelper
    {
        public static void ETAll()
        {
            //有顺序的 别乱动
            ET_Init_RepairDependencies();
            ET_Loader_ReGenerateProjectFiles();
            ET_Loader_ReGenerateProjectAssemblyReference();
            ET_Loader_UpdateScriptsReferences();
            ET_Excel_ExcelExporter();
            ET_Proto_Proto2CS();
        }

        public static void ET_Init_RepairDependencies()
        {
            ExecuteMenuItem("ET/Init/RepairDependencies");
        }

        public static void ET_Loader_ReGenerateProjectFiles()
        {
            ExecuteMenuItem("ET/Loader/ReGenerateProjectFiles");
        }

        public static void ET_Loader_ReGenerateProjectAssemblyReference()
        {
            ExecuteMenuItem("ET/Loader/ReGenerateProjectAssemblyReference");
        }

        public static void ET_Loader_UpdateScriptsReferences()
        {
            ExecuteMenuItem("ET/Loader/UpdateScriptsReferences");
        }

        public static void ET_Excel_ExcelExporter()
        {
            //关闭此功能 改为手动执行
            //ExecuteMenuItem("ET/Excel/ExcelExporter");
        }

        public static void ET_Proto_Proto2CS()
        {
            ExecuteMenuItem("ET/Proto/Proto2CS");
        }

        public static void ExecuteMenuItem(string menuItem)
        {
            try
            {
                EditorApplication.ExecuteMenuItem(menuItem);
            }
            catch (Exception e)
            {
                Debug.LogError($"执行错误 {menuItem} 请检查 {e.Message}");
            }
        }
    }
}