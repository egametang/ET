using System.Collections.Generic;
using System.IO;

namespace ETModel
{
    public class EditorResHelper
    {
        /// <summary>
        /// 获取文件夹内所有的预制跟场景路径
        /// </summary>
        /// <param name="srcPath">源文件夹</param>
        /// <param name="subDire">是否获取子文件夹</param>
        /// <returns></returns>
        public static List<string> GetPrefabsAndScenes(string srcPath)
        {
            List<string> paths = new List<string>();
            FileHelper.GetAllFiles(paths, srcPath);
            
            List<string> files = new List<string>();
            foreach (string str in paths)
            {
                if (str.EndsWith(".prefab") || str.EndsWith(".unity"))
                {
                    files.Add(str);
                }
            }
            return files;
        }
        public static List<string>  GetSmallGame(string srcPath)
        {
            List<string> paths = new List<string>();
            FileHelper.GetAllFiles(paths, srcPath);
            
            List<string> files = new List<string>();
            foreach (string str in paths)
            {
                if (str.EndsWith(".prefab") || str.EndsWith(".unity")
                    )
                {
                    files.Add(str);
                }
            }
            return files;
        }
        /**
         * 获取路径内的显示资源;
         */
        public static List<string> GetSmallGameAtlas(string srcPath)
        {
            List<string> paths = new List<string>();
            FileHelper.GetAllFiles(paths, srcPath);
            
            List<string> files = new List<string>();
            foreach (string str in paths)
            {
                if (str.EndsWith(".TTF") || str.EndsWith(".mat")
                    || str.EndsWith(".physicMaterial")
                    || str.EndsWith(".png")|| str.EndsWith(".jpg")
                )
                {
                    files.Add(str);
                }
            }
            return files;
        }
        /**
         * 获取路径内的声音文件;
         */
        public static List<string> GetSmallGameSound(string srcPath)
        {
            List<string> paths = new List<string>();
            FileHelper.GetAllFiles(paths, srcPath);
            
            List<string> files = new List<string>();
            foreach (string str in paths)
            {
                if (str.EndsWith(".mp3") || str.EndsWith(".wav")
                )
                {
                    files.Add(str);
                }
            }
            return files;
        }
        /// <summary>
        /// 获取文件夹内所有资源路径
        /// </summary>
        /// <param name="srcPath">源文件夹</param>
        /// <param name="subDire">是否获取子文件夹</param>
        /// <returns></returns>
        public static List<string> GetAllResourcePath(string srcPath, bool subDire)
        {
            List<string> paths = new List<string>();
            string[] files = Directory.GetFiles(srcPath);
            foreach (string str in files)
            {
                if (str.EndsWith(".meta"))
                {
                    continue;
                }
                paths.Add(str);
            }
            if (subDire)
            {
                foreach (string subPath in Directory.GetDirectories(srcPath))
                {
                    List<string> subFiles = GetAllResourcePath(subPath, true);
                    paths.AddRange(subFiles);
                }
            }
            return paths;
        }
    }
}
