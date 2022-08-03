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
            Mode.CreateScript => "�����ű�",
            Mode.CreateFolder => "����Ŀ¼",
            Mode.RenameFolder => "�������ű�",
            Mode.RenamScript => "������Ŀ¼",
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
            Mode.CreateScript => "�����ű�",
            Mode.CreateFolder => "����Ŀ¼",
            Mode.RenameFolder => "�������ű�",
            Mode.RenamScript => "������Ŀ¼",
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
                            EditorUtility.DisplayDialog("����", "������ֲ��Ϸ�", "�ر�");
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
                            EditorUtility.DisplayDialog("����", "������ֲ��Ϸ�", "�ر�");
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
                            EditorUtility.DisplayDialog("����", "������ֲ��Ϸ�", "�ر�");
                            return;
                        }
                        if (!_textFieldStr.EndsWith(".cs"))
                        {
                            _textFieldStr += ".cs";
                        }
                        string dest = Path.Combine(file.Directory.FullName, _textFieldStr);

                        if (File.Exists(dest))
                        {
                            EditorUtility.DisplayDialog("����", "����Ѵ���ͬ���ļ�", "�ر�");
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
                            EditorUtility.DisplayDialog("����", "������ֲ��Ϸ�", "�ر�");
                            return;
                        }
                        DirectoryInfo dirInfo = new DirectoryInfo(_fullPath);
                        string dest = Path.Combine(dirInfo.Parent.FullName, _textFieldStr);

                        if (File.Exists(dest))
                        {
                            EditorUtility.DisplayDialog("����", "����Ѵ���ͬ���ļ�", "�ر�");
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
