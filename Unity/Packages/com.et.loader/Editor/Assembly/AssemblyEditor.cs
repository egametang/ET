using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class AssemblyEditor
    {
        private static readonly string[] DllNames = { "ET.Hotfix", "ET.HotfixView", "ET.Model", "ET.ModelView" };
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.playModeStateChanged += change =>
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                    {
                        OnExitingEditMode();
                        break;
                    }
                }
            };
        }

        /// <summary>
        /// 退出编辑模式时处理(即将进入运行模式)
        /// EnableDll模式时, 屏蔽掉Library的dll(通过改文件后缀方式屏蔽), 仅使用Define.CodeDir下的dll
        /// </summary>
        static void OnExitingEditMode()
        {
            foreach (string dll in DllNames)
            {
                string dllFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.dll";
                if (File.Exists(dllFile))
                {
                    File.Delete(dllFile);
                }

                string pdbFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.pdb";
                if (File.Exists(pdbFile))
                {
                    File.Delete(pdbFile);
                }
            }
        }
    }
}