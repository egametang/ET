using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor ;
using UnityEngine;
 

namespace HybridCLR.Editor.Installer
{
    public class InstallerWindow : EditorWindow
    {
        private InstallerController m_Controller;

        [MenuItem("HybridCLR/Installer...", false, 0)]
        private static void Open()
        {
            InstallerWindow window = GetWindow<InstallerWindow>("HybridCLR Installer", true);
            window.minSize = new Vector2(800f, 500f);
        }

        private void OnEnable()
        {
            m_Controller = new InstallerController();
        }

        private void OnGUI()
        {
            string minCompatibleVersion = m_Controller.GetMinCompatibleVersion(m_Controller.Il2CppBranch);
            GUI.enabled = true;
            GUILayout.Space(10f);
            EditorGUILayout.LabelField("=======================说明====================");
            EditorGUILayout.LabelField(
                $"你所在项目的Unity版本可以与il2cpp_plus版本:{m_Controller.Il2CppBranch} 不一样。\n"
                + $"如果你的Unity的版本号 >= {minCompatibleVersion}, 可以直接安装。\n"
                + $"如果你的Unity的版本号 < {minCompatibleVersion}, \n"
                + $"由于安装HybridCLR时需要从il2cpp_plus对应版本{m_Controller.Il2CppBranch}（而不是你项目版本）拷贝il2cpp目录，\n"
                + $"你必须同时安装相应版本 {m_Controller.Il2CppBranch} 才能完成安装", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("==============================================");
            GUILayout.Space(10f);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"安装状态：{(m_Controller.HasInstalledHybridCLR() ? "已安装" : "未安装")}", EditorStyles.boldLabel);
            GUILayout.Space(5f);
            EditorGUILayout.LabelField($"当前Unity版本: {Application.unityVersion}，匹配的il2cpp_plus分支: {m_Controller.Il2CppBranch}");
            GUISelectUnityDirectory($"il2cpp_plus分支对应Unity版本的il2cpp路径", "Select");
            GUILayout.Space(10f);
            GUIInstallButton("安装最新HybridCLR插件代码到本项目", "安装", InitHybridCLR);
            EditorGUILayout.EndVertical();
        }

        private void GUIInstallButton(string content, string button, Action onClick)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content);
            GUI.enabled = m_Controller.CheckValidIl2CppInstallDirectory(m_Controller.Il2CppBranch, m_Controller.Il2CppInstallDirectory) == InstallErrorCode.Ok;
            if (GUILayout.Button(button, GUILayout.Width(100)))
            {
                onClick?.Invoke();
                GUIUtility.ExitGUI();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
        }


        private void GUISelectUnityDirectory(string content, string selectButton)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content, GUILayout.MaxWidth(300));
            string il2cppInstallDirectory = m_Controller.Il2CppInstallDirectory = EditorGUILayout.TextField(m_Controller.Il2CppInstallDirectory);
            if (GUILayout.Button(selectButton, GUILayout.Width(100)))
            {
                string temp = EditorUtility.OpenFolderPanel(content, m_Controller.Il2CppInstallDirectory, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    il2cppInstallDirectory = m_Controller.Il2CppInstallDirectory = temp;
                }
            }
            EditorGUILayout.EndHorizontal();

            InstallErrorCode err = m_Controller.CheckValidIl2CppInstallDirectory(m_Controller.Il2CppBranch, il2cppInstallDirectory);
            switch (err)
            {
                case InstallErrorCode.Ok:
                    {
                        break;
                    }
                case InstallErrorCode.Il2CppInstallPathNotExists:
                    {
                        EditorGUILayout.HelpBox("li2cpp 路径不存在", MessageType.Error);
                        break;
                    }
                case InstallErrorCode.Il2CppInstallPathNotMatchIl2CppBranch:
                    {
                        EditorGUILayout.HelpBox($"il2cpp 版本不兼容，最小版本为 {m_Controller.GetMinCompatibleVersion(m_Controller.Il2CppBranch)}", MessageType.Error);
                        break;
                    }
                case InstallErrorCode.NotIl2CppPath:
                    {
                        EditorGUILayout.HelpBox($"当前选择的路径不是il2cpp目录（必须类似 xxx/il2cpp）", MessageType.Error);
                        break;
                    }
                default: throw new Exception($"not support {err}");
            }
        }

        private void InitHybridCLR()
        {
            m_Controller.InitHybridCLR(m_Controller.Il2CppBranch, m_Controller.Il2CppInstallDirectory);
        }
    }
}
