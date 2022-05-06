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
                content =  content.Replace("<Compile Include=\"Assets\\Hotfix\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\Hotfix\\Unity.Hotfix.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.HotfixView.csproj"))
            {
                content =  content.Replace("<Compile Include=\"Assets\\HotfixView\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\HotfixView\\Unity.HotfixView.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.Model.csproj"))
            {
                content =  content.Replace("<Compile Include=\"Assets\\Model\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\Model\\Unity.Model.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.ModelView.csproj"))
            {
                content =  content.Replace("<Compile Include=\"Assets\\ModelView\\Empty.cs\" />", string.Empty);
                content =  content.Replace("<None Include=\"Assets\\ModelView\\Unity.ModelView.asmdef\" />", string.Empty);
            }
            
            if (path.EndsWith("Unity.Hotfix.csproj"))
            {
                return GenerateCustomProject(path, content, @"Codes\Hotfix\**\*.cs");
            }

            if (path.EndsWith("Unity.HotfixView.csproj"))
            {
                return GenerateCustomProject(path, content, @"Codes\HotfixView\**\*.cs");
            }

            if (path.EndsWith("Unity.Model.csproj"))
            {
                return GenerateCustomProject(path, content, @"Codes\Model\**\*.cs");
            }

            if (path.EndsWith("Unity.ModelView.csproj"))
            {
                return GenerateCustomProject(path, content, @"Codes\ModelView\**\*.cs");
            }
            
            return content;
        }

        private static string GenerateCustomProject(string path, string content, string codesPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);

            var newDoc = doc.Clone() as XmlDocument;

            var rootNode = newDoc.GetElementsByTagName("Project")[0];

            var itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
            var compile = newDoc.CreateElement("Compile", newDoc.DocumentElement.NamespaceURI);

            compile.SetAttribute("Include", codesPath);
            itemGroup.AppendChild(compile);

            var projectReference = newDoc.CreateElement("ProjectReference", newDoc.DocumentElement.NamespaceURI);
            projectReference.SetAttribute("Include", @"..\Tools\Analyzer\Analyzer.csproj");
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