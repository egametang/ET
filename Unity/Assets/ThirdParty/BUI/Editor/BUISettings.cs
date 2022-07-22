using System.IO;
using UnityEditor;
using UnityEngine;

namespace BUI
{
    public class BUISettings : EditorWindow
    {
        [MenuItem("BUI/BUI Settings")]
        public static void OpenWindow()
        {
            BUISettings window = GetWindow<BUISettings>();
            window.minSize = new Vector2(600, 200);
            window.maxSize = new Vector2(600, 200);
            window.Show();
        }
        public static string settingsObjectPath = "Assets/BUISettingsObject/BUISettingsObject.asset";
        private string _templatePath;
        private string _outputFolder;

        private BUISettingsObject _settingsObject;
        private void OnGUI()
        {
            if (_settingsObject == null)
            {
                var asset = AssetDatabase.LoadAssetAtPath<BUISettingsObject>(settingsObjectPath);
                if (asset)
                {
                    _settingsObject = asset;
                }
                else
                {
                    if (GUILayout.Button("创建SettingsObject", GUILayout.Height(200)))
                    {
                        asset = CreateInstance<BUISettingsObject>();
                        FileInfo file = new FileInfo(settingsObjectPath);
                        if (!file.Directory.Exists)
                        {
                            file.Directory.Create();
                        }
                        AssetDatabase.CreateAsset(asset, settingsObjectPath);
                        _settingsObject = asset;
                    }
                    return;
                }
            }
            //===========================
            GUILayout.BeginHorizontal();
            GUILayout.Label("Asset", GUILayout.Width(80));
            EditorGUILayout.ObjectField(_settingsObject, typeof(BUISettingsObject), false);
            GUILayout.EndHorizontal();
            //===========================
            GUILayout.BeginHorizontal();
            GUILayout.Label("模板路径", GUILayout.Width(80));
            GUILayout.TextField(_settingsObject.templatePath, GUILayout.Width(420));
            if (GUILayout.Button("选择模板"))
            {
                _settingsObject.templatePath = EditorUtility.OpenFilePanel("选择模板", Application.dataPath, "txt");
            }
            GUILayout.EndHorizontal();
            //===========================
            GUILayout.BeginHorizontal();
            GUILayout.Label("输出目录", GUILayout.Width(80));
            GUILayout.TextField(_settingsObject.outputFolder, GUILayout.Width(420));
            if (GUILayout.Button("选择目录"))
            {
                _settingsObject.outputFolder = EditorUtility.OpenFolderPanel("选择目录", Application.dataPath, "");
            }
            GUILayout.EndHorizontal();
        }
    }
}
