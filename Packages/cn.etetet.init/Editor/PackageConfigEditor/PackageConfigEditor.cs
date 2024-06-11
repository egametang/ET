using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [CustomEditor(typeof(PackageConfig))]
    public class PackageConfigEditor: Editor
    {
        [MenuItem("ET/Create All PackageType")]
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

                string dir = Application.dataPath + "/../" + $"{Path.GetDirectoryName(path)}" + "/Scripts/Model/Share/";
                CreatePackageTypeFile(dir, config);
            }
            Debug.Log("Generate PackageType File Finish!");
        }

        public static void CreatePackageTypeFile(string dir, PackageConfig config)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using FileStream fileStream = new($"{dir}/PackageType.cs", FileMode.Create);
            using StreamWriter streamWriter = new(fileStream);
            streamWriter.WriteLine("namespace ET");
            streamWriter.WriteLine("{");
            streamWriter.WriteLine("    public static partial class PackageType");
            streamWriter.WriteLine("    {");
            streamWriter.WriteLine("        public const int " + config.Name + " = " + config.Id + ";");
            streamWriter.WriteLine("    }");
            streamWriter.WriteLine("}");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Create PackageType"))
            {
                PackageConfig packageConfig = (PackageConfig)this.target;
                if (!packageConfig.CreatePackageTypeFile)
                {
                    return;
                }
                string path = AssetDatabase.GetAssetPath(packageConfig);
                CreatePackageTypeFile(Path.Combine(Path.GetDirectoryName(path), "Scripts/Model/Share/"), packageConfig);
            }
        }
    }
}