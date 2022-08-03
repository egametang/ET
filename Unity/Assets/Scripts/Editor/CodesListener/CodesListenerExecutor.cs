using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
public class CodesListenerExecutor
{
    public enum CodeClass
    {
        Client,
        Server
    }
    public static void Refresh()
    {
        //客户端Model
        RegisterCodeFolder(CodeClass.Client, 
            "Unity.Model.csproj", "../Codes", 
            new Dictionary<string, List<string>>() { 
                { "Model", new List<string>() { "Client", "Share" } }, 
                { "Generate", new List<string> { "Client", "Share" } } });
        //客户端Hotfix
        RegisterCodeFolder(CodeClass.Client, 
            "Unity.Hotfix.csproj", "../Codes", 
            new Dictionary<string, List<string>>() { 
                { "Hotfix", new List<string>() { "Client", "Share" } } });
        //客户端ModelView
        RegisterCodeFolder(CodeClass.Client, 
            "Unity.ModelView.csproj", "../Codes", 
            new Dictionary<string, List<string>>() { 
                { "ModelView", new List<string>() { "Client" } } });
        //客户端HotfixView
        RegisterCodeFolder(CodeClass.Client, 
            "Unity.HotfixView.csproj", "../Codes", 
            new Dictionary<string, List<string>>() { 
                { "HotfixView", new List<string>() { "Client" } } });
        //服务端Model
        RegisterCodeFolder(CodeClass.Server, 
            "../DotNet/Model/DotNet.Model.csproj", "../Codes", 
            new Dictionary<string, List<string>>() { 
                { "Model", new List<string>() { "Server", "Share" ,"Client"} }, 
                { "Generate", new List<string> { "Server", "Share" } } });
        //服务端Hotfix
        RegisterCodeFolder(CodeClass.Server, 
            "../DotNet/Hotfix/DotNet.Hotfix.csproj", "../Codes", 
            new Dictionary<string, List<string>>() { 
                { "Hotfix", new List<string>() { "Server", "Share","Client" } } });
    }

    public static void RegisterCodeFolder(CodeClass codeClass, string csprojPath, string rootFolder, Dictionary<string, List<string>> asmNames)
    {
        List<FileInfo> files = new List<FileInfo>();
        GetAssemblyCodeFiles(new DirectoryInfo(rootFolder), files, asmNames, string.Empty, 0);
        var doc = Adjust(codeClass, rootFolder, csprojPath, files);
        doc.Save(csprojPath);
        string final = HandleSomeThing(csprojPath, File.ReadAllText(csprojPath));
        FileInfo fileInfo = new FileInfo(csprojPath);
        File.WriteAllText(fileInfo.FullName, final);
    }

    private static string HandleSomeThing(string path, string content)
    {
        if (path.EndsWith("Unity.Hotfix.csproj"))
        {
            content = content.Replace("<Compile Include=\"Assets\\Scripts\\Hotfix\\Empty.cs\" />", string.Empty);
            content = content.Replace("<None Include=\"Assets\\Scripts\\Hotfix\\Unity.Hotfix.asmdef\" />", string.Empty);
        }

        if (path.EndsWith("Unity.HotfixView.csproj"))
        {
            content = content.Replace("<Compile Include=\"Assets\\Scripts\\HotfixView\\Empty.cs\" />", string.Empty);
            content = content.Replace("<None Include=\"Assets\\Scripts\\HotfixView\\Unity.HotfixView.asmdef\" />", string.Empty);
        }

        if (path.EndsWith("Unity.Model.csproj"))
        {
            content = content.Replace("<Compile Include=\"Assets\\Scripts\\Model\\Empty.cs\" />", string.Empty);
            content = content.Replace("<None Include=\"Assets\\Scripts\\Model\\Unity.Model.asmdef\" />", string.Empty);
        }

        if (path.EndsWith("Unity.ModelView.csproj"))
        {
            content = content.Replace("<Compile Include=\"Assets\\Scripts\\ModelView\\Empty.cs\" />", string.Empty);
            content = content.Replace("<None Include=\"Assets\\Scripts\\ModelView\\Unity.ModelView.asmdef\" />", string.Empty);
        }
        return content;
    }

    private static void GetAssemblyCodeFiles(DirectoryInfo parent, List<FileInfo> files, Dictionary<string, List<string>> asmNames, string key, int depth)
    {
        if (depth > 1)
        {
            return;
        }
        foreach (var dir in parent.GetDirectories())
        {
            if (depth == 0 && asmNames.Keys.Contains(dir.Name))
            {
                //第一层通过，进入第二层
                GetAssemblyCodeFiles(dir, files, asmNames, dir.Name, 1);
            }
            else if (depth == 1 && asmNames.TryGetValue(key, out var second) && second.Contains(dir.Name))
            {
                foreach (var file in dir.GetFiles("*.cs", SearchOption.AllDirectories))
                {
                    files.Add(file);
                }
            }
            else
            {
                GetAssemblyCodeFiles(dir, files, asmNames, key, depth);
            }
        }
    }

    private static string analyzerPath = @"..\Codes\Analyzer\Share.Analyzer.csproj";
    public static XDocument Adjust(CodeClass codeClass, string workPlace, string csprojPath, List<FileInfo> files)
    {
        DirectoryInfo workPlaceDir = new DirectoryInfo(workPlace);
        workPlace = workPlaceDir.FullName.Replace("/", "\\");
        XDocument doc = XDocument.Load(csprojPath);
        XElement project = doc.Elements().First(e => e.Name.LocalName == "Project");
        var comments = doc.Nodes().Where(n => n.NodeType == System.Xml.XmlNodeType.Comment).ToList();
        foreach (var c in comments)
        {
            c.Remove();
        }
        var itemGroups = project.Elements().Where(e => e.Name.LocalName == "ItemGroup");
        List<XElement> delCompile = new List<XElement>();
        foreach (XElement itemGroup in itemGroups)
        {
            foreach (var element in itemGroup.Elements())
            {
                if (element.Name.LocalName == "Compile")
                {
                    delCompile.Add(element);
                }
                if (element.Name.LocalName == "ProjectReference")
                {
                    XAttribute includeAttr = element.Attribute("Include");
                    if (includeAttr != null && includeAttr.Value == analyzerPath)
                    {
                        delCompile.Add(element);
                    }
                }
            }
        }
        foreach (XElement item in delCompile)
        {
            item.Remove();
        }
        foreach (var csFile in files)
        {
            string fullPath = csFile.FullName.Replace("/", "\\");
            XElement firstItemGroup = project.Elements().First(e => e.Name.LocalName == "ItemGroup");
            XElement compile = new XElement(project.Name.ToString().Replace("Project", "Compile"), new XAttribute("Include", fullPath));
            XElement link = new XElement(project.Name.ToString().Replace("Project", "Link"));
            link.SetValue(fullPath.Substring(workPlace.Length + 1));
            compile.Add(link);
            firstItemGroup.Add(compile);
        }
        XElement lastItemGroup = project.Elements().Last(e => e.Name.LocalName == "ItemGroup");
        XElement projectReference = new XElement(project.Name.ToString().Replace("Project", "ProjectReference"));
        projectReference.SetAttributeValue("Include", analyzerPath);
        projectReference.SetAttributeValue("OutputItemType", @"Analyzer");
        projectReference.SetAttributeValue("ReferenceOutputAssembly", @"false");
        if (codeClass == CodeClass.Client)
        {
            XElement projectItem = new XElement(project.Name.ToString());
            projectItem.Value = @"{d1f2986b-b296-4a2d-8f12-be9f470014c3}";
            projectReference.Add(projectItem);
        }
        lastItemGroup.Add(projectReference);
        return doc;
    }
}
