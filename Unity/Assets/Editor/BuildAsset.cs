using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Model;

public class BuildAsset
{
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<string> EncryptionFilesName = new List<string>();

    
    [MenuItem("Tools/Build Asset", false, 11)]
    public static void BuildiPhoneResource()
    {
        BuildTarget target = BuildTarget.StandaloneWindows;
        if (Application.platform == RuntimePlatform.Android) { target = BuildTarget.Android; }
        if (Application.platform == RuntimePlatform.IPhonePlayer) { target = BuildTarget.iOS; }
        BuildAssetResource(target);
    }

    /// <summary>
    /// 编译AssetResource
    /// </summary>
    public static void BuildAssetResource(BuildTarget target)
    {
        Caching.ClearCache();
        string resPath = AppDataPath + "/" + AppConst.StreamingAssetsName + "/";
        if (Directory.Exists(resPath)) { Directory.Delete(resPath, true); }

        string dataPath = PathHelp.AppResPath;
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);
        BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.ChunkBasedCompression, target);

        paths.Clear();
        files.Clear();
        Recursive(resPath);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

}
