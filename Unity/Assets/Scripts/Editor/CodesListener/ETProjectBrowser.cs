using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;
public class ETProjectBrowser : EditorWindow
{
    //[MenuItem("ET/ET Project")]
    public static ETProjectBrowser Open()
    {
        var window = GetWindow<ETProjectBrowser>("ET Project");
        window.Show();
        return window;
    }
    private string _settingsAssetPath = "Assets/Resources/ETProjectBrowserSettings.asset";
    private ETProjectBrowserSettings _settings;
    private List<string> _ignoreDirs = new List<string>() { "Analyzer" };
    private List<string> _selectedPaths;
    private TreeNode _root;
    private FileSystemWatcher _watcher;
    private Texture2D _texture;

    private void OnEnable()
    {
        _settings = AssetDatabase.LoadAssetAtPath<ETProjectBrowserSettings>(_settingsAssetPath);
        _selectedPaths = new List<string>(10);
        StartListening();
        _texture = Resources.Load<Texture2D>("background");
    }

    private void StartListening()
    {
        if (_settings != null)
        {
            _watcher = new FileSystemWatcher(_settings.ListenFolderInfo.FullName);
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;
            _watcher.Created += SetTreeNodes;
            _watcher.Deleted += SetTreeNodes;
        }
        Refresh();
    }
    private void OnDisable()
    {
        if (_watcher != null)
        {
            _watcher.Dispose();
        }
    }
    //Layout members
    private void OnGUI()
    {
        if (_settings == null)
        {
            if (GUILayout.Button("请创建配置文件"))
            {
                FileInfo fi = new FileInfo(_settingsAssetPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                _settings = CreateInstance<ETProjectBrowserSettings>();
                AssetDatabase.CreateAsset(_settings, _settingsAssetPath);
            }
            return;
        }
        if (string.IsNullOrEmpty(_settings.ListenFolderPath))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("请至少指定一个ET Project目录");
            GUILayout.EndHorizontal();
            return;
        }
        else
        {
            GUILayout.BeginHorizontal();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            int _ = 0;
            int min = Mathf.RoundToInt(_scrollPos.y / 20);
            int max = min + 20;
            DrawTreeNodes(_root, ref _, min, max);
            EditorGUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }
    }

    private (string name, DateTime time) _clickToken;
    private TreeNode currentNode;
    private Vector2 _scrollPos;

    private void DrawTreeNodes(TreeNode parent, ref int index, int min, int max)
    {
        foreach (var node in parent.Children)
        {
            bool render = true;
            index++;
            if (node.Type == TreeNode.NodeType.Folder)
            {
                if (render)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10 * node.Level);
                    if (GUILayout.Button("…", GUILayout.MaxWidth(20)))
                    {
                        Event evt = Event.current;
                        Vector2 pos = evt.mousePosition;
                        ETProjectBrowserMenusItem.Path = node.FullName;
                        ETProjectBrowserMenusItem.Refresh = EditFolder;
                        EditorUtility.DisplayPopupMenu(new Rect(pos.x, pos.y, 0, 0), "ET/ET Project Menus/Folder", null);
                    }
                    var icon = EditorGUIUtility.IconContent("Folder Icon");
                    if (node.Children == null || node.Children.Count == 0)
                    {
                        icon = EditorGUIUtility.IconContent("FolderEmpty Icon");
                    }
                    icon.text = node.Name;
                    node.Selected = EditorGUILayout.Foldout(node.Selected, icon, true);
                    if (node.Selected)
                    {
                        if (!_selectedPaths.Contains(node.FullName))
                        {
                            _selectedPaths.Add(node.FullName);
                        }
                    }
                    else
                    {
                        if (_selectedPaths.Contains(node.FullName))
                        {
                            _selectedPaths.Remove(node.FullName);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (node.Selected)
                {
                    DrawTreeNodes(node, ref index, min, max);
                }
            }
            else
            {
                if (render)
                {
                    var style = new GUIStyle();
                    if (currentNode == node)
                    {
                        style.normal.background = _texture;
                    }
                    EditorGUILayout.BeginHorizontal(style);
                    GUILayout.Space(10 * node.Level);
                    if (GUILayout.Button("…", GUILayout.MaxWidth(20)))
                    {
                        Event evt = Event.current;
                        Vector2 pos = evt.mousePosition;
                        ETProjectBrowserMenusItem.Path = node.FullName;
                        ETProjectBrowserMenusItem.Refresh = EditFolder;
                        EditorUtility.DisplayPopupMenu(new Rect(pos.x, pos.y, 0, 0), "ET/ET Project Menus/Script", null);
                    }
                    GUILayout.Label(EditorGUIUtility.IconContent("cs Script Icon"), GUILayout.MaxHeight(20), GUILayout.MaxWidth(20));
                    if (GUILayout.Button(node.Name, new GUIStyle("Label")))
                    {
                        currentNode = node;
                        DateTime lastClickTime = _clickToken.time;
                        if ((DateTime.Now - lastClickTime).TotalSeconds < 0.5 && !string.IsNullOrEmpty(_clickToken.name) && _clickToken.name == node.FullName)
                        {
                            CodeEditor.CurrentEditor.OpenProject(node.FullName);
                        }
                        _clickToken = (node.FullName, DateTime.Now);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
    }

    private void EditFolder(string dirName)
    {
        if (!string.IsNullOrEmpty(dirName))
        {
            if (!_selectedPaths.Contains(dirName))
            {
                _selectedPaths.Add(dirName);
            }
        }
        Refresh();
    }

    private void Refresh(bool force = false)
    {
        SetTreeNodes(null, null);
        //if (_autoFresh || force)
        //{
        //    RefreshCsprojs();
        //}
    }
    private void RefreshCsprojs()
    {
        CodesListenerExecutor.Refresh();
    }
    private void SetTreeNodes(object sender, FileSystemEventArgs e)
    {
        if (!Directory.Exists(_settings.ListenFolderPath))
        {
            return;
        }
        TreeNode root = new TreeNode(_settings.ListenFolderInfo.FullName);
        List<TreeNode> nodes = new List<TreeNode>(512) { root };
        List<string> selectedPaths = new List<string>(10);
        GetTreeNodes(root, nodes, selectedPaths, 0);
        _root = root;
        _selectedPaths = selectedPaths;
    }


    private void GetTreeNodes(TreeNode parent, List<TreeNode> list, List<string> selectedPaths, int level)
    {
        if (!parent.Exist)
        {
            return;
        }
        //深度优先,文件夹在上面
        foreach (string dir in Directory.GetDirectories(parent.FullName))
        {
            if (_ignoreDirs.Contains(Path.GetFileName(dir)))
            {
                continue;
            }
            TreeNode node = new TreeNode(parent, dir, TreeNode.NodeType.Folder);
            if (_selectedPaths.Contains(dir))
            {
                node.Selected = true;
                selectedPaths.Add(dir);
            }
            list.Add(node);
            node.Level = level;
            GetTreeNodes(node, list, selectedPaths, level + 1);
            node.Parent = parent;
        }
        foreach (string dir in Directory.GetFiles(parent.FullName, "*.cs"))
        {
            if (_ignoreDirs.Contains(Path.GetFileName(dir)))
            {
                continue;
            }
            TreeNode node = new TreeNode(parent, dir, TreeNode.NodeType.Item);
            node.Parent = parent;
            node.Level = level;
            list.Add(node);
        }
    }
}

public class TreeNode
{
    public string Name { get; }
    public int Level { get; set; }
    public string FullName { get; }
    public NodeType Type { get; }
    public bool Selected { get; set; }
    public List<TreeNode> Children { get; }
    public TreeNode Parent { get; set; }
    public bool Exist => Directory.Exists(FullName);

    public enum NodeType
    {
        Folder,
        Item,
    }
    public TreeNode(string path)
    {
        Name = Path.GetFileName(path);
        FullName = path;
        Type = NodeType.Folder;
        Children = new List<TreeNode>();
    }
    public TreeNode(TreeNode parent, string path, NodeType type)
    {
        Parent = parent;
        Name = Path.GetFileName(path);
        FullName = path;
        Type = type;
        if (type == NodeType.Folder)
        {
            Children = new List<TreeNode>();
        }
        Parent.AddChild(this);
    }

    private void AddChild(TreeNode node)
    {
        Children.Add(node);
    }
    public override string ToString()
    {
        return FullName;
    }
}