using UnityEditor;
using System.Diagnostics;

namespace ET
{
    public static class ExcelEditor
    {
        [MenuItem("ET/Excel/ExcelExporter")]
        public static void Run()
        {
            Process process = ProcessHelper.DotNet("./Packages/cn.etetet.excel/DotNet~/Exe/ET.ExcelExporter.dll", "./", true);

            UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
        }

        public static void Init()
        {
            Run();
        }
    }
}