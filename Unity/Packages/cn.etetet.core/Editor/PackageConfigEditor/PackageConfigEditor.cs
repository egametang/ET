using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class PackageConfigEditor
    {
        [MenuItem("ET/Generate PackageType File")]
        public static void GeneratePackageTypeFile()
        {
            string[] packageConfigs = AssetDatabase.FindAssets("t:PackageConfig");
            foreach (string packageConfig in packageConfigs)
            {
                string path = AssetDatabase.GUIDToAssetPath(packageConfig);
                PackageConfig config = AssetDatabase.LoadAssetAtPath<PackageConfig>(path);
                if (!config.CreatePackageTypeFile)
                {
                    continue;
                }

                string packageTypePath = Application.dataPath + "/../" + $"{Path.GetDirectoryName(path)}" + "/Scripts/Model/Share/PackageType.cs";
                string dir = Path.GetDirectoryName(packageTypePath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                using FileStream fileStream = new(packageTypePath, FileMode.Create);
                using StreamWriter streamWriter = new(fileStream);
                streamWriter.WriteLine("namespace ET");
                streamWriter.WriteLine("{");
                streamWriter.WriteLine("    public static partial class PackageType");
                streamWriter.WriteLine("    {");
                streamWriter.WriteLine("        public const int " + config.Name + " = " + config.Id + ";");
                streamWriter.WriteLine("    }");
                streamWriter.WriteLine("}");
            }
            Log.Debug("Generate PackageType File Finish!");
        }
    }
}