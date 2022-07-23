
using ET;
using FairyGUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class FGUICodeGenerator
{
    private const string ATTR_TEMPLATE = "[FGUIField]";
    private const string ATTR_CUSTOM_TEMPLATE = "[FGUIWrappedView]";
    private const string FIELD_TEMPLATE = "public #TYPE# #NAME#;";
    private const string TEMPLATE_PATH = "Assets/Editor/FGUIEditor/template.txt";
    private const string GENERATE_CODE_DIR = "/Unity/Codes/ModelView/Main/UI";
    private const string BASE_PACKAGE_PATH = "Assets/Bundles/FGUI/Main_fui.bytes";
    private const string BASE_PACKAGE_NAME = "ABaseComponents_fui";

    private static bool _simulateFirst = true;
    private static List<string> _generatingCodes = new List<string>();
    public static List<(string packageName, string name, string url, string fields)> _codes = new List<(string packageName, string name, string url, string fields)>();


    [MenuItem("Assets/生成FGUI组件代码")]
    public static void Generate()
    {
        try
        {
            _generatingCodes.Clear();
            _codes.Clear();
            UIPackage.RemoveAllPackages();
            var sources = Selection.GetFiltered<TextAsset>(SelectionMode.Assets).Where(ta => ta.name.EndsWith("_fui"));
            if (!sources.Any(ta => ta.name == BASE_PACKAGE_NAME))
            {
                TextAsset baseAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(BASE_PACKAGE_PATH);
                UIPackage.AddPackage(baseAsset.bytes, baseAsset.name.Replace("_fui", ""), (string name, string extension, System.Type type, PackageItem item) => { });
            }
            foreach (var source in sources)
            {
                UIPackage package = UIPackage.AddPackage(source.bytes, source.name.Replace("_fui", ""), (string name, string extension, System.Type type, PackageItem item) => { });
                foreach (var item in package.GetItems())
                {
                    if (!item.exported)
                    {
                        continue;
                    }
                    GComponent obj = package.CreateObject(item.name).asCom;
                    try
                    {
                        if (obj != null)
                        {
                            HandleComponent(obj, package.name, item.name, _simulateFirst);
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        obj.Dispose();
                    }
                }
                if (_simulateFirst)
                {
                    string compDirPath = Application.dataPath.Replace("/Unity/Assets", GENERATE_CODE_DIR);
                    string dirPath = $"{compDirPath}/{package.name}";
                    DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
                    if (directoryInfo.Exists)
                    {
                        FileHelper.CleanDirectory(directoryInfo.FullName);
                    }
                    foreach (var code in _codes)
                    {
                        WriteScript(code.packageName, code.name, code.url, code.fields);
                    }
                }
            }
            EditorUtility.DisplayDialog("", "生成FGUI代码成功", "确定");
        }
        finally
        {
            _generatingCodes.Clear();
            UIPackage.RemoveAllPackages();
        }
    }
    private static bool HandleComponent(GComponent root, string packageName, string name, bool simulate = false)
    {
        try
        {
            StringBuilder sb = new StringBuilder();
            List<string> lines = new List<string>();
            string url = root.resourceURL;

            foreach (Controller controller in root.Controllers)
            {
                sb.AppendLine("\t\t" + ATTR_TEMPLATE);
                lines.Add("\t\t" + ATTR_TEMPLATE);
                string field = "\t\t" + FIELD_TEMPLATE.Replace("#TYPE#", typeof(Controller).Name).Replace("#NAME#", controller.name
                    );
                sb.AppendLine(field);
                lines.Add(field);
            }
            foreach (Transition transition in root.Transitions)
            {
                sb.AppendLine("\t\t" + ATTR_TEMPLATE);
                lines.Add("\t\t" + ATTR_TEMPLATE);
                string field = "\t\t" + FIELD_TEMPLATE.Replace("#TYPE#", typeof(Transition).Name).Replace("#NAME#", transition.name
                    );
                sb.AppendLine(field);
                lines.Add(field);
            }
            foreach (GObject obj in root.GetChildren())
            {
                if (obj.id.StartsWith(obj.name + "_"))
                {
                    continue;
                }
                bool hasComponentClass = false;
                if (obj.GetType() == typeof(GComponent))
                {
                    if (string.IsNullOrEmpty(obj.resourceURL))
                    {
                        Debug.LogError($"{packageName}包的组件{name}中{obj.name}引用来源不符合规范,请检查一下");
                        return false;
                    }
                    GComponent com = UIPackage.CreateObjectFromURL(obj.resourceURL).asCom;
                    hasComponentClass = HandleComponent(com, com.packageItem.owner.name, com.packageItem.name, simulate);
                    if (hasComponentClass)
                    {
                        string typeStr = com.packageItem.name;
                        sb.AppendLine("\t\t" + ATTR_CUSTOM_TEMPLATE);
                        lines.Add("\t\t" + ATTR_CUSTOM_TEMPLATE);
                        string field = "\t\t" + FIELD_TEMPLATE.Replace("#TYPE#", typeStr).Replace("#NAME#", obj.name
                            );
                        sb.AppendLine(field);
                        lines.Add(field);
                    }
                    com.Dispose();
                }
                if (!hasComponentClass)
                {
                    string typeStr = obj.GetType().Name;
                    sb.AppendLine("\t\t" + ATTR_TEMPLATE);
                    lines.Add("\t\t" + ATTR_TEMPLATE);
                    string field = "\t\t" + FIELD_TEMPLATE.Replace("#TYPE#", typeStr).Replace("#NAME#", obj.name
                        );
                    sb.AppendLine(field);
                    lines.Add(field);
                }

            }
            if (simulate)
            {
                _codes.Add((packageName, name, url, string.Join("\r\n", lines)));
            }
            else if (!string.IsNullOrEmpty(sb.ToString()))
            {
                WriteScript(packageName, name, url, string.Join("\r\n", lines));
                return true;
            }
            else
            {
                return false;
            }

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
        return true;
    }

    private static string InsertStr(string origin, string content)
    {
        return origin.Replace("#FIELDS#", content);
    }
    private static void WriteScript(string packageName, string resName, string url, string fields)
    {
        string compDirPath = Application.dataPath.Replace("/Unity/Assets", GENERATE_CODE_DIR);
        string dirPath = $"{compDirPath}/{packageName}";
        string path = $"{dirPath}/{resName}.cs";
        if (_generatingCodes.Contains(path))
        {
            return;
        }
        _generatingCodes.Add(path);
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        if (!File.Exists(path))
        {
            File.Create(path).Dispose();
        }
        string content = File.ReadAllText(path);
        content = InsertStr(content, fields);
        if (string.IsNullOrEmpty(content))
        {
            string template = File.ReadAllText(TEMPLATE_PATH);
            content = template.Replace("#CLASS_NAME#", resName)
                .Replace("#URL#", $"\"{url}\"")
                .Replace("#PACKAGE_NAME#", $"\"{packageName}\"")
                .Replace("#RES_NAME#", $"\"{resName}\"");
            content = InsertStr(content, fields);
        }
        FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        fs.SetLength(0);
        byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
        fs.Write(contentBytes, 0, contentBytes.Length);
        fs.Close();
    }
}
