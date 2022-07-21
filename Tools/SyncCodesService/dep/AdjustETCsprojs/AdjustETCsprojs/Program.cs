// See https://aka.ms/new-console-template for more information
using System.Xml.Linq;
string command = args[0];
if (command == "refresh")
{
    string root = args[1].Trim();
    AdjustTool.Adjust(root + @"\Unity.Model.csproj", "Model");
    AdjustTool.Adjust(root + @"\Unity.ModelView.csproj", "ModelView");
    AdjustTool.Adjust(root + @"\Unity.Hotfix.csproj", "Hotfix");
    AdjustTool.Adjust(root + @"\Unity.HotfixView.csproj", "HotfixView");
}
else if (command == "clear")
{
    string root = args[1].Trim();
    AdjustTool.Adjust(root + @"\Unity.Model.csproj", "Model", false);
    AdjustTool.Adjust(root + @"\Unity.ModelView.csproj", "ModelView", false);
    AdjustTool.Adjust(root + @"\Unity.Hotfix.csproj", "Hotfix", false);
    AdjustTool.Adjust(root + @"\Unity.HotfixView.csproj", "HotfixView", false);
}
//string root = @"D:\Github\HoH\Unity";



public class AdjustTool
{
    public static void Adjust(string csprojPath, string srcDir, bool addComment = true)
    {
        string includeValue = $"Codes\\{srcDir}\\**\\*.cs";
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
            }
        }
        foreach (XElement item in delCompile)
        {
            item.Remove();
        }
        XElement firstItemGroup = project.Elements().First(e => e.Name.LocalName == "ItemGroup");
        XElement compile = new XElement(project.Name.ToString().Replace("Project", "Compile"), new XAttribute("Include", includeValue));
        firstItemGroup.Add(compile);
        if (addComment)
        {
            XComment xComment = new XComment(DateTime.Now.ToString());
            doc.Add(xComment);
        }
        doc.Save(csprojPath);
        Console.WriteLine($"刷新{srcDir}成功");
    }
}