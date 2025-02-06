using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace HybridCLR.Editor.Installer
{
    public class InstallerWindow : EditorWindow
    {
        private InstallerController _controller;

        private bool _installFromDir;

        private string _installLibil2cppWithHybridclrSourceDir;

        private void OnEnable()
        {
            _controller = new InstallerController();
        }

        private void OnGUI()
        {
            var rect = new Rect
            {
                x = EditorGUIUtility.currentViewWidth - 24,
                y = 5,
                width = 24,
                height = 24
            };
            var content = EditorGUIUtility.IconContent("Settings");
            content.tooltip = "HybridCLR Settings";
            if (GUI.Button(rect, content, GUI.skin.GetStyle("IconButton")))
            {
                SettingsService.OpenProjectSettings("Project/HybridCLR Settings");
            }

            bool hasInstall = _controller.HasInstalledHybridCLR();

            GUILayout.Space(10f);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Installed: {hasInstall}", EditorStyles.boldLabel);
            GUILayout.Space(10f);

            EditorGUILayout.LabelField($"Package Version:   v{_controller.PackageVersion}");
            GUILayout.Space(5f);
            EditorGUILayout.LabelField($"Installed Version:    v{_controller.InstalledLibil2cppVersion ?? " Unknown"}");
            GUILayout.Space(5f);

            GUILayout.Space(10f);

            InstallerController.CompatibleType compatibleType = _controller.GetCompatibleType();
            if (compatibleType != InstallerController.CompatibleType.Incompatible)
            {
                if (compatibleType == InstallerController.CompatibleType.MaybeIncompatible)
                {
                    EditorGUILayout.HelpBox($"Maybe incompatible with current version, recommend minimum compatible version:{_controller.GetCurrentUnityVersionMinCompatibleVersionStr()}", MessageType.Warning);
                }

                EditorGUILayout.BeginHorizontal();
                _installFromDir = EditorGUILayout.Toggle("Copy libil2cpp from local", _installFromDir, GUILayout.MinWidth(100));
                EditorGUI.BeginDisabledGroup(!_installFromDir);
                EditorGUILayout.TextField(_installLibil2cppWithHybridclrSourceDir, GUILayout.Width(400));
                if (GUILayout.Button("Choose", GUILayout.Width(100)))
                {
                    _installLibil2cppWithHybridclrSourceDir = EditorUtility.OpenFolderPanel("Select libil2cpp", Application.dataPath, "libil2cpp");
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20f);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Install", GUILayout.Width(100)))
                {
                    InstallLocalHybridCLR();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox($"Incompatible with current version, minimum compatible version:{_controller.GetCurrentUnityVersionMinCompatibleVersionStr()}", MessageType.Error);
            }

            EditorGUILayout.EndVertical();
        }

        private void InstallLocalHybridCLR()
        {
            if (_installFromDir)
            {
                if (!Directory.Exists(_installLibil2cppWithHybridclrSourceDir))
                {
                    Debug.LogError($"Source libil2cpp:'{_installLibil2cppWithHybridclrSourceDir}' doesn't exist.");
                    return;
                }
                if (!File.Exists($"{_installLibil2cppWithHybridclrSourceDir}/il2cpp-config.h") || !File.Exists($"{_installLibil2cppWithHybridclrSourceDir}/hybridclr/RuntimeApi.cpp"))
                {
                    Debug.LogError($"Source libil2cpp:' {_installLibil2cppWithHybridclrSourceDir} ' is invalid");
                    return;
                }
                _controller.InstallFromLocal(_installLibil2cppWithHybridclrSourceDir);
            }
            else
            {
                _controller.InstallDefaultHybridCLR();
            }
        }
    }
}
