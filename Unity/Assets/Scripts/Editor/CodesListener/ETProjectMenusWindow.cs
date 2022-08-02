using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ETProjectMenusWindow : EditorWindow
{
    public enum Mode
    {
        CreateScript,
        CreateFolder,
        RenameFolder,
        RenamScript
    }
    public static ETProjectMenusWindow Open(string fullPath, Mode mode, Action<string> refresh)
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return null;
        }
        string title = (mode) switch
        {
            Mode.CreateScript => "创建脚本",
            Mode.CreateFolder => "创建目录",
            Mode.RenameFolder => "重命名脚本",
            Mode.RenamScript => "重命名目录",
            _ => mode.ToString(),
        };
        var window = GetWindow<ETProjectMenusWindow>(false, title);
        window._fullPath = fullPath;
        window._mode = mode;
        window._refresh = refresh;
        window.minSize = new Vector2(200, 40);
        window.maxSize = new Vector2(200, 40);

        window.Show();
        return window;
    }
    string _fullPath;
    private Mode _mode;
    string _textFieldStr = string.Empty;
    private Action<string> _refresh;
    private bool _focus = false;

    private void OnEnable()
    {
        _focus = true;
    }
    private void OnDisable()
    {
        ETProjectBrowserMenusItem.Path = string.Empty;
        ETProjectBrowserMenusItem.Refresh = null;
    }
    private void OnGUI()
    {
        GUI.SetNextControlName("TextField");
        _textFieldStr = GUILayout.TextField(_textFieldStr, new GUIStyle("TextField") { alignment = TextAnchor.MiddleCenter });
        if (_focus)
        {
            EditorGUI.FocusTextInControl("TextField");
            _focus = false;
        }
        string btnName = (_mode) switch
        {
            Mode.CreateScript => "创建脚本",
            Mode.CreateFolder => "创建目录",
            Mode.RenameFolder => "重命名脚本",
            Mode.RenamScript => "重命名目录",
            _ => _mode.ToString(),
        };
        if (GUILayout.Button(btnName))
        {
            switch (_mode)
            {
                case Mode.CreateScript:
                    {
                        if (_textFieldStr.Contains("/") || _textFieldStr.Contains("\\"))
                        {
                            EditorUtility.DisplayDialog("错误", "这个名字不合法", "关闭");
                            return;
                        }
                        if (!_textFieldStr.EndsWith(".cs"))
                        {
                            _textFieldStr += ".cs";
                        }
                        DirectoryInfo dirInfo = new DirectoryInfo(_fullPath);
                        string name = Path.GetFileNameWithoutExtension(_textFieldStr);
                        string template = File.ReadAllText("Assets/Scripts/Editor/CodesListener/template.txt");
                        File.WriteAllText(Path.Combine(dirInfo.FullName, _textFieldStr), template.Replace("#CLASS_NAME#", name));
                        _refresh?.Invoke(dirInfo.FullName);
                        Close();
                        break;
                    }
                case Mode.CreateFolder:
                    {
                        if (_textFieldStr.Contains("/") || _textFieldStr.Contains("\\"))
                        {
                            EditorUtility.DisplayDialog("错误", "这个名字不合法", "关闭");
                            return;
                        }
                        DirectoryInfo dirInfo = new DirectoryInfo(_fullPath);
                        Directory.CreateDirectory(Path.Combine(dirInfo.FullName, _textFieldStr));
                        _refresh?.Invoke(dirInfo.FullName);
                        Close();
                        break;
                    }
                case Mode.RenamScript:
                    {
                        FileInfo file = new FileInfo(_fullPath);
                        if (_textFieldStr.Contains("/") || _textFieldStr.Contains("\\"))
                        {
                            EditorUtility.DisplayDialog("错误", "这个名字不合法", "关闭");
                            return;
                        }
                        if (!_textFieldStr.EndsWith(".cs"))
                        {
                            _textFieldStr += ".cs";
                        }
                        string dest = Path.Combine(file.Directory.FullName, _textFieldStr);

                        if (File.Exists(dest))
                        {
                            EditorUtility.DisplayDialog("错误", "这个已存在同名文件", "关闭");
                            return;
                        }
                        File.Move(file.FullName, dest);
                        _refresh?.Invoke(null);
                        break;
                    }
                case Mode.RenameFolder:
                    {
                        if (_textFieldStr.Contains("/") || _textFieldStr.Contains("\\"))
                        {
                            EditorUtility.DisplayDialog("错误", "这个名字不合法", "关闭");
                            return;
                        }
                        DirectoryInfo dirInfo = new DirectoryInfo(_fullPath);
                        string dest = Path.Combine(dirInfo.Parent.FullName, _textFieldStr);

                        if (File.Exists(dest))
                        {
                            EditorUtility.DisplayDialog("错误", "这个已存在同名文件", "关闭");
                            return;
                        }
                        Directory.Move(dirInfo.FullName, dest);
                        _refresh?.Invoke(dest);
                        Close();
                        break;
                    }
            }
        }

    }
}
