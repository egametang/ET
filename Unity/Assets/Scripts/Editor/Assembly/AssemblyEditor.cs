using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class AssemblyEditor
    {
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.playModeStateChanged += change =>
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingEditMode:
                        OnExitingEditMode();
                        break;
                    case PlayModeStateChange.ExitingPlayMode:
                        OnExitingPlayMode();
                        break;
                }
            };
        }

        /// <summary>
        /// 退出编辑模式时处理(即将进入运行模式)
        /// EnableDll模式时, 屏蔽掉Library的dll(通过改文件后缀方式屏蔽), 仅使用Define.CodeDir下的dll
        /// </summary>
        static void OnExitingEditMode()
        {
            // 保证进入运行模式前编译好代码
            if (!AssemblyTool.IsCompileFinished)
            {
                AssemblyTool.DoCompile();

                // 编译错误, 不允许进入游戏
                if (!AssemblyTool.IsCompileFinished)
                {
                    EditorApplication.isPlaying = false;
                    return;
                }
            }

            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            if (!globalConfig.EnableDll)
                return;

            foreach (string dll in AssemblyTool.DllNames)
            {
                string dllFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.dll";
                if (File.Exists(dllFile))
                {
                    string dllDisableFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.dll.DISABLE";
                    if (File.Exists(dllDisableFile))
                        File.Delete(dllDisableFile);

                    File.Move(dllFile, dllDisableFile);
                }

                string pdbFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.pdb";
                if (File.Exists(pdbFile))
                {
                    string pdbDisableFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.pdb.DISABLE";
                    if (File.Exists(pdbDisableFile))
                        File.Delete(pdbDisableFile);

                    File.Move(pdbFile, pdbDisableFile);
                }
            }
        }

        /// <summary>
        /// 退出运行模式时处理(即将进入编辑模式)
        /// 还原Library里面屏蔽掉的dll(HybridCLR或者非EnableDll模式都会用到这个目录下的dll, 故需要还原)
        /// </summary>
        static void OnExitingPlayMode()
        {
            foreach (string dll in AssemblyTool.DllNames)
            {
                string dllFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.dll";
                string dllDisableFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.dll.DISABLE";
                if (File.Exists(dllFile))
                {
                    if (File.Exists(dllDisableFile))
                        File.Delete(dllDisableFile);
                }
                else
                {
                    if (File.Exists(dllDisableFile))
                        File.Move(dllDisableFile, dllFile);
                }

                string pdbDisableFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.pdb.DISABLE";
                string pdbFile = $"{Application.dataPath}/../Library/ScriptAssemblies/{dll}.pdb";
                if (File.Exists(pdbFile))
                {
                    if (File.Exists(pdbDisableFile))
                        File.Delete(pdbDisableFile);
                }
                else
                {
                    if (File.Exists(pdbDisableFile))
                        File.Move(pdbDisableFile, pdbFile);
                }
            }
        }
    }
}