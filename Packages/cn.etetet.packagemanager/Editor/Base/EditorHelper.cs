using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ET.PackageManager.Editor
{
    /// <summary>
    /// 编辑器工具可能会用到的各种工具
    /// </summary>
    public static class EditorHelper
    {
        private static string m_ProjFolder;

        /// <summary>
        /// 得到项目绝对路径
        /// Application.dataPath = D:\XX\Assets
        /// 这里去掉最后的Assets，得到D:\XX
        /// </summary>
        /// <returns></returns>
        public static string GetProjPath(string relativePath = "")
        {
            if (string.IsNullOrEmpty(m_ProjFolder))
            {
                var projFolder = Application.dataPath;
                m_ProjFolder = projFolder.Substring(0, projFolder.Length - 7);
            }

            if (relativePath == null)
            {
                relativePath = "";
            }

            relativePath = relativePath.Trim();
            if (!string.IsNullOrEmpty(relativePath))
            {
                if (relativePath.Contains("\\"))
                {
                    relativePath = relativePath.Replace("\\", "/");
                }

                if (!relativePath.StartsWith("/"))
                {
                    relativePath = $"/{relativePath}";
                }
            }

            return $"{m_ProjFolder}{relativePath}";
        }

        /// <summary>
        /// 打开文件或文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFileOrFolder(string path)
        {
            Process.Start("explorer.exe", path.Replace("/", "\\"));
        }

        /// <summary>
        /// 用于查找某个只知道名字的文件在什么位置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string[] GetAssetPaths(string name)
        {
            var paths = AssetDatabase.FindAssets(name);
            for (int i = 0; i < paths.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(paths[i]);
            }

            return paths;
        }

        /// <summary>
        /// 在项目文件内写入文件
        /// </summary>
        public static bool WriteAllText(string path, string contents, bool log = false)
        {
            try
            {
                path = GetProjPath(path);
                var dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    Debug.LogError("dir == null");
                    return false;
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, contents, Encoding.UTF8);
                if (log)
                    Debug.Log(path + "创建成功");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("写入文件失败: path =" + path + ", err=" + e);
                return false;
            }
        }

        public static string ReadAllText(string path)
        {
            try
            {
                path = GetProjPath(path);
                if (!File.Exists(path))
                {
                    return null;
                }

                return File.ReadAllText(path);
            }
            catch (Exception e)
            {
                Debug.LogError("读取文件失败: path =" + path + ", err=" + e);
                return null;
            }
        }

        public static bool WriteAllBytes(string path, byte[] bytes, bool log = false)
        {
            try
            {
                path = GetProjPath(path);
                var dir = Path.GetDirectoryName(path);
                if (dir == null)
                {
                    Debug.LogError("dir == null");
                    return false;
                }

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllBytes(path, bytes);
                if (log)
                    Debug.Log(path + "创建成功");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("写入文件失败: path =" + path + ", err=" + e);
                return false;
            }
        }

        public static byte[] ReadAllBytes(string path)
        {
            try
            {
                path = GetProjPath(path);
                if (!File.Exists(path))
                {
                    return null;
                }

                return File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Debug.LogError("读取文件失败: path =" + path + ", err=" + e);
                return null;
            }
        }

        /// <summary>
        /// 是否忽略
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFileIgnore(string path)
        {
            return path.EndsWith(".meta")
                 || path.EndsWith(".manifest")
                 || path.Contains(".DS_Store");
        }

        // ----------------------------------------------------------------------------------------
        /// <summary>
        /// 进度条界面更新
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">内容</param>
        /// <param name="current">当前进度</param>
        /// <param name="total">总进度</param>
        public static void DisplayProgressBar(string title, string message, int current, int total)
        {
            float progress = 0;
            if (total != 0)
            {
                progress = Mathf.InverseLerp(0, total, current);
                message  = string.Format("{0} {1}/{2}", message, current + 1, total);
            }

            EditorUtility.DisplayProgressBar(title, message, progress);
        }

        /// <summary>
        /// 关闭进度
        /// </summary>
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }

        //检查目标路径文件夹是否存在
        public static bool ExistsDirectory(string path)
        {
            return Directory.Exists(GetProjPath(path));
        }

        //检查文件夹 不存在则创建
        public static void CreateExistsDirectory(string path, bool checkDirectory = false)
        {
            path = GetProjPath(path);
            if (checkDirectory)
            {
                path = Path.GetDirectoryName(path);
                if (path == null)
                {
                    Debug.LogError($" {path} dir == null");
                    return;
                }
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}