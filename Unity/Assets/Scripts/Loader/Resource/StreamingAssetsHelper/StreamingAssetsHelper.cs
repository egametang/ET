using System.IO;
using UnityEngine;
using YooAsset;
#if !UNITY_EDITOR
using System.Collections.Generic;
#endif

namespace ET
{
    /// <summary>
    /// 资源文件查询服务类
    /// </summary>
    public class GameQueryServices : IBuildinQueryServices
    {
        /// <summary>
        /// 查询内置文件的时候，是否比对文件哈希值
        /// </summary>
        public static bool CompareFileCRC = false;

        public bool Query(string packageName, string fileName, string fileCRC)
        {
            // 注意：fileName包含文件格式
            return StreamingAssetsHelper.FileExists(packageName, fileName, fileCRC);
        }
    }

#if UNITY_EDITOR
    public static class StreamingAssetsHelper
    {
        public static void Init()
        {
        }

        public static bool FileExists(string packageName, string fileName, string fileCRC)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, StreamingAssetsDefine.RootFolderName, packageName, fileName);
            if (File.Exists(filePath))
            {
                if (GameQueryServices.CompareFileCRC)
                {
                    string crc32 = YooAsset.Editor.EditorTools.GetFileCRC32(filePath);
                    return crc32 == fileCRC;
                }

                return true;
            }

            return false;
        }
    }
#else
    public static class StreamingAssetsHelper
    {
        private class PackageQuery
        {
            public readonly Dictionary<string, BuildinFileManifest.Element> Elements = new(1000);
        }

        private static bool _isInit;
        private static readonly Dictionary<string, PackageQuery> _packages = new(10);

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            if (_isInit)
            {
                return;
            }

            _isInit = true;

            var manifest = Resources.Load<BuildinFileManifest>("BuildinFileManifest");
            if (manifest != null)
            {
                foreach (var element in manifest.BuildinFiles)
                {
                    if (_packages.TryGetValue(element.PackageName, out PackageQuery package) == false)
                    {
                        package = new PackageQuery();
                        _packages.Add(element.PackageName, package);
                    }

                    package.Elements.Add(element.FileName, element);
                }
            }
        }

        /// <summary>
        /// 内置文件查询方法
        /// </summary>
        public static bool FileExists(string packageName, string fileName, string fileCRC32)
        {
            if (!_isInit)
            {
                Init();
            }

            if (!_packages.TryGetValue(packageName, out PackageQuery package))
            {
                return false;
            }

            if (!package.Elements.TryGetValue(fileName, out var element))
            {
                return false;
            }

            if (GameQueryServices.CompareFileCRC)
            {
                return element.FileCRC32 == fileCRC32;
            }

            return true;
        }
    }
#endif

#if UNITY_EDITOR
    internal class PreprocessBuild : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        /// <summary>
        /// 在构建应用程序前处理
        /// 原理：在构建APP之前，搜索StreamingAssets目录下的所有资源文件，然后将这些文件信息写入内置清单，内置清单存储在Resources文件夹下。
        /// </summary>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            string saveFilePath = "Assets/Resources/BuildinFileManifest.asset";
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
                UnityEditor.AssetDatabase.SaveAssets();
                UnityEditor.AssetDatabase.Refresh();
            }

            string folderPath = $"{Application.dataPath}/StreamingAssets/{StreamingAssetsDefine.RootFolderName}";
            DirectoryInfo root = new DirectoryInfo(folderPath);
            if (!root.Exists)
            {
                Debug.LogWarning($"没有发现YooAsset内置目录 : {folderPath}");
                return;
            }

            var manifest = ScriptableObject.CreateInstance<BuildinFileManifest>();
            FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
            foreach (var fileInfo in files)
            {
                if (fileInfo.Extension == ".meta")
                {
                    continue;
                }

                if (fileInfo.Name.StartsWith("PackageManifest_"))
                {
                    continue;
                }

                BuildinFileManifest.Element element = new() { PackageName = fileInfo.Directory.Name, FileCRC32 = YooAsset.Editor.EditorTools.GetFileCRC32(fileInfo.FullName), FileName = fileInfo.Name };
                manifest.BuildinFiles.Add(element);
            }

            if (!Directory.Exists("Assets/Resources"))
            {
                Directory.CreateDirectory("Assets/Resources");
            }

            UnityEditor.AssetDatabase.CreateAsset(manifest, saveFilePath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"一共{manifest.BuildinFiles.Count}个内置文件，内置资源清单保存成功 : {saveFilePath}");
        }
    }
#endif
}