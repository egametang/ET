using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YIUI.Numeric.Editor
{
    public static class CopyFolder
    {
        public static bool Copy(string sourceFolder, string targetFolder)
        {
            DirectoryInfo sourceDir = new(sourceFolder);
            DirectoryInfo targetDir = new(targetFolder);

            var result = false;
            if (Directory.Exists(targetFolder))
            {
                UnityTipsHelper.CallBack($"{targetFolder} 已存在!!!\n \n警告将会执行全覆盖操作!!!", () =>
                {
                    Directory.Delete(targetFolder, true);
                    result = Start();
                });
            }
            else
            {
                result = Start();
            }

            return result;

            bool Start()
            {
                try
                {
                    Directory.CreateDirectory(targetFolder);
                    CopyAll(sourceDir, targetDir);
                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"{targetFolder} 创建错误 {e.Message}");
                }

                return false;
            }
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (!target.Exists)
            {
                target.Create();
            }

            foreach (FileInfo fi in source.GetFiles())
            {
                string targetFilePath = Path.Combine(target.FullName, fi.Name);
                fi.CopyTo(targetFilePath, true);
            }

            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }
    }
}