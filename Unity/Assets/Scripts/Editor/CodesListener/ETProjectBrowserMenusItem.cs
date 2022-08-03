using System;
using System.IO;
using UnityEditor;

public class ETProjectBrowserMenusItem
{
    public static string Path;
    public static Action<string> Refresh;
    [MenuItem("ET/ET Project Menus/Folder/创建脚本")]
    public static void CreateScript()
    {
        ETProjectMenusWindow.Open(Path, ETProjectMenusWindow.Mode.CreateScript, Refresh);
    }
    [MenuItem("ET/ET Project Menus/Script/重命名脚本")]
    public static void RenameScript()
    {
        ETProjectMenusWindow.Open(Path, ETProjectMenusWindow.Mode.RenamScript, Refresh);
    }
    [MenuItem("ET/ET Project Menus/Script/删除脚本")]
    public static void DeleteScript()
    {
        FileInfo fileInfo = new FileInfo(Path);
        if (EditorUtility.DisplayDialog("删除文件", $"确定删除{fileInfo.Name}吗", "确定", "不了"))
        {
            File.Delete(fileInfo.FullName);
            Refresh?.Invoke(null);
        }
    }
    [MenuItem("ET/ET Project Menus/Folder/创建目录")]
    public static void CreateFolder()
    {
        ETProjectMenusWindow.Open(Path, ETProjectMenusWindow.Mode.CreateFolder, Refresh);
    }
    [MenuItem("ET/ET Project Menus/Folder/重命名目录")]
    public static void RenameFolder()
    {
        ETProjectMenusWindow.Open(Path, ETProjectMenusWindow.Mode.RenameFolder, Refresh);
    }
    [MenuItem("ET/ET Project Menus/Folder/删除目录")]
    public static void DeleteFolder()
    {
        DirectoryInfo dirInfo = new DirectoryInfo(Path);

        if (EditorUtility.DisplayDialog("删除文件夹", $"确定删除{dirInfo.Name}吗", "确定", "不了"))
        {
            Directory.Delete(dirInfo.FullName, true);
            Refresh?.Invoke(null);
        }
    }
}
