using System;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

namespace ET
{
    public class OnGenerateCSProjectProcessor : AssetPostprocessor
    {
        public static string OnGeneratedCSProject(string path, string content)
        {
            
            if (path.EndsWith("Unity.Hotfix.csproj"))
            {
                content =  content.Replace("<Compile Include=\"Assets\\Scripts\\Hotfix\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\Scripts\\Hotfix\\Unity.Hotfix.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.HotfixView.csproj"))
            {
                content =  content.Replace("<Compile Include=\"Assets\\Scripts\\HotfixView\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\Scripts\\HotfixView\\Unity.HotfixView.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.Model.csproj"))
            {
                content =  content.Replace("<Compile Include=\"Assets\\Scripts\\Model\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\Scripts\\Model\\Unity.Model.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.ModelView.csproj"))
            {
                content =  content.Replace("<Compile Include=\"Assets\\Scripts\\ModelView\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\Scripts\\ModelView\\Unity.ModelView.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.Hotfix.csproj"))
            {
                return GenerateCustomProject(path, content, 
                    @"..\Codes\Hotfix\Client\**\*.cs Client", 
                    @"..\Codes\Hotfix\Share\**\*.cs Share");
            }

            if (path.EndsWith("Unity.HotfixView.csproj"))
            {
                return GenerateCustomProject(path, content, 
                    @"..\Codes\HotfixView\Client\**\*.cs Client", 
                    @"..\Codes\HotfixView\Share\**\*.cs Share"); 
            }

            if (path.EndsWith("Unity.Model.csproj"))
            {
                return GenerateCustomProject(path, content, 
                    @"..\Codes\Model\Client\**\*.cs Client",
                    @"..\Codes\Model\Share\**\*.cs Share",
                    @"..\Codes\Generate\Client\**\*.cs Generate");
            }

            if (path.EndsWith("Unity.ModelView.csproj"))
            {
                return GenerateCustomProject(path, content, 
                    @"..\Codes\ModelView\Client\**\*.cs Client",
                    @"..\Codes\ModelView\Share\**\*.cs Share");
            }
            
            return content;
        }

        private static string GenerateCustomProject(string path, string content, params string[] links)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            var newDoc = doc.Clone() as XmlDocument;

            var rootNode = newDoc.GetElementsByTagName("Project")[0];

            XmlElement itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
            foreach (var s in links)
            {
                string[] ss = s.Split(' ');
                string p = ss[0];
                string linkStr = ss[1];
                XmlElement compile = newDoc.CreateElement("Compile", newDoc.DocumentElement.NamespaceURI);
                XmlElement link = newDoc.CreateElement("Link", newDoc.DocumentElement.NamespaceURI);
                link.InnerText = $"{linkStr}\\%(RecursiveDir)%(FileName)%(Extension)";
                compile.AppendChild(link);
                compile.SetAttribute("Include", p);
                itemGroup.AppendChild(compile);
            }

            var projectReference = newDoc.CreateElement("ProjectReference", newDoc.DocumentElement.NamespaceURI);
            projectReference.SetAttribute("Include", @"..\Codes\Analyzer\Share.Analyzer.csproj");
            projectReference.SetAttribute("OutputItemType", @"Analyzer");
            projectReference.SetAttribute("ReferenceOutputAssembly", @"false");

            var project = newDoc.CreateElement("Project", newDoc.DocumentElement.NamespaceURI);
            project.InnerText = @"{d1f2986b-b296-4a2d-8f12-be9f470014c3}";
            projectReference.AppendChild(project);

            var name = newDoc.CreateElement("Name", newDoc.DocumentElement.NamespaceURI);
            name.InnerText = "Analyzer";
            projectReference.AppendChild(project);

            itemGroup.AppendChild(projectReference);

            rootNode.AppendChild(itemGroup);

            using (StringWriter sw = new StringWriter())
            {

                using (XmlTextWriter tx = new XmlTextWriter(sw))
                {
                    tx.Formatting = Formatting.Indented;
                    newDoc.WriteTo(tx);
                    tx.Flush();
                    return sw.GetStringBuilder().ToString();
                }
            }
        }
    }
}