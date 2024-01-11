#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace YIUIFramework.Editor
{
    /// <summary>
    /// 奥丁扩展
    /// </summary>
    public static class OdinMenuTree_Extensions
    {
        //仿照源码扩展的 根据 AssetImporter 类型
        public static IEnumerable<OdinMenuItem> AddAllAssetImporterAtPath(
            this OdinMenuTree tree,
            string            menuPath,
            string            assetFolderPath,
            System.Type       type,
            bool              includeSubDirectories = false,
            bool              flattenSubDirectories = false)
        {
            assetFolderPath = (assetFolderPath ?? "").TrimEnd('/') + "/";
            string lower = assetFolderPath.ToLower();
            if (!lower.StartsWith("assets/") && !lower.StartsWith("packages/"))
                assetFolderPath = "Assets/" + assetFolderPath;
            assetFolderPath = assetFolderPath.TrimEnd('/') + "/";
            IEnumerable<string> strings = ((IEnumerable<string>)AssetDatabase.GetAllAssetPaths()).Where<string>(
                (Func<string, bool>)(x =>
                {
                    if (includeSubDirectories)
                        return x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase);
                    return string.Compare(Sirenix.Utilities.PathUtilities.GetDirectoryName(x).Trim('/'),
                        assetFolderPath.Trim('/'), true) == 0;
                }));
            menuPath = menuPath ?? "";
            menuPath = menuPath.TrimStart('/');
            HashSet<OdinMenuItem> result = new HashSet<OdinMenuItem>();
            foreach (string str1 in strings)
            {
                var assetImporter = AssetImporter.GetAtPath(str1);
                if (assetImporter != null && type.IsInstanceOfType(assetImporter))
                {
                    string withoutExtension = Path.GetFileNameWithoutExtension(str1);
                    string path             = menuPath;
                    if (!flattenSubDirectories)
                    {
                        string str2 =
                            (Sirenix.Utilities.PathUtilities.GetDirectoryName(str1).TrimEnd('/') + "/").Substring(
                                assetFolderPath.Length);
                        if (str2.Length != 0)
                            path = path.Trim('/') + "/" + str2;
                    }

                    path = path.Trim('/') + "/" + withoutExtension;
                    string name;
                    SplitMenuPath(path, out path, out name);
                    tree.AddMenuItemAtPath((ICollection<OdinMenuItem>)result, path,
                        new OdinMenuItem(tree, name, (object)@assetImporter));
                }
            }

            return (IEnumerable<OdinMenuItem>)result;
        }

        private static void SplitMenuPath(string menuPath, out string path, out string name)
        {
            menuPath = menuPath.Trim('/');
            int length = menuPath.LastIndexOf('/');
            if (length == -1)
            {
                path = "";
                name = menuPath;
            }
            else
            {
                path = menuPath.Substring(0, length);
                name = menuPath.Substring(length + 1);
            }
        }
    }
}
#endif