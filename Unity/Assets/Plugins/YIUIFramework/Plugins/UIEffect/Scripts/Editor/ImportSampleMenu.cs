#if !UNITY_2019_1_OR_NEWER
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Coffee.UIEffects
{
    public static class ImportSampleMenu
    {
        private const string jsonGuid = "546af75b6221c4768be79d67c9cea1fb";

        [MenuItem("Assets/Samples/UIEffect/Import Demo")]
        private static void ImportDemo()
        {
            ImportSample(jsonGuid, "Demo");
        }

        private static void ImportSample(string jsonGuid, string sampleName)
        {
            var jsonPath = AssetDatabase.GUIDToAssetPath(jsonGuid);
            var json = File.ReadAllText(jsonPath);
            var version = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]+)\"").Groups[1].Value;
            var displayName = Regex.Match(json, "\"displayName\"\\s*:\\s*\"([^\"]+)\"").Groups[1].Value;
            var src = string.Format("{0}/Samples~/{1}", Path.GetDirectoryName(jsonPath), sampleName);
            var srcAlt = string.Format("{0}/Samples/{1}", Path.GetDirectoryName(jsonPath), sampleName);
            var dst = string.Format("Assets/Samples/{0}/{1}/{2}", displayName, version, sampleName);
            var previousPath = GetPreviousSamplePath(displayName, sampleName);

            // Remove the previous sample directory.
            if (!string.IsNullOrEmpty(previousPath))
            {
                var msg = "A different version of the sample is already imported at\n\n"
                          + previousPath
                          + "\n\nIt will be deleted when you update. Are you sure you want to continue?";
                if (!EditorUtility.DisplayDialog("Sample Importer", msg, "OK", "Cancel"))
                    return;

                FileUtil.DeleteFileOrDirectory(previousPath);

                var metaFile = previousPath + ".meta";
                if (File.Exists(metaFile))
                    FileUtil.DeleteFileOrDirectory(metaFile);
            }

            if (!Directory.Exists(dst))
                FileUtil.DeleteFileOrDirectory(dst);

            var dstDir = Path.GetDirectoryName(dst);
            if (!Directory.Exists(dstDir))
                Directory.CreateDirectory(dstDir);

            if (Directory.Exists(src))
                FileUtil.CopyFileOrDirectory(src, dst);
            else if (Directory.Exists(srcAlt))
                FileUtil.CopyFileOrDirectory(srcAlt, dst);
            else
                throw new DirectoryNotFoundException(src);

            AssetDatabase.Refresh(ImportAssetOptions.ImportRecursive);
        }

        private static string GetPreviousSamplePath(string displayName, string sampleName)
        {
            var sampleRoot = string.Format("Assets/Samples/{0}", displayName);
            var sampleRootInfo = new DirectoryInfo(sampleRoot);
            if (!sampleRootInfo.Exists) return null;

            return sampleRootInfo.GetDirectories()
                .Select(versionDir => Path.Combine(versionDir.ToString(), sampleName))
                .FirstOrDefault(Directory.Exists);
        }
    }
}
#endif
