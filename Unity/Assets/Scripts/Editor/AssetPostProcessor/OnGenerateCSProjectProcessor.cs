using System;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;

namespace ET
{
    public class OnGenerateCSProjectProcessor: AssetPostprocessor
    {
        public static string OnGeneratedCSProject(string path, string content)
        {
            if (path.EndsWith("Unity.Core.csproj"))
            {
                return GenerateCustomProject(path, content);
            }

            if (Define.EnableCodes)
            {
                if (path.EndsWith("Unity.Hotfix.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }

                if (path.EndsWith("Unity.Model.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }

                if (path.EndsWith("Unity.HotfixView.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }

                if (path.EndsWith("Unity.ModelView.Codes.csproj"))
                {
                    content = GenerateCustomProject(path, content);
                }
            }
            else
            {
                if (path.EndsWith("Unity.Hotfix.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\Hotfix\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\Hotfix\\Unity.Hotfix.asmdef\" />", string.Empty);

                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\Hotfix\**\*.cs %(RecursiveDir)%(FileName)%(Extension)");
                }

                if (path.EndsWith("Unity.HotfixView.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\HotfixView\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\HotfixView\\Unity.HotfixView.asmdef\" />", string.Empty);
                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\HotfixView\**\*.cs %(RecursiveDir)%(FileName)%(Extension)");
                }

                if (path.EndsWith("Unity.Model.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\Model\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\Model\\Unity.Model.asmdef\" />", string.Empty);
                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\Model\Server\**\*.cs Server\%(RecursiveDir)%(FileName)%(Extension)",
                        @"Assets\Scripts\Codes\Model\Client\**\*.cs Client\%(RecursiveDir)%(FileName)%(Extension)",
                        @"Assets\Scripts\Codes\Model\Share\**\*.cs Share\%(RecursiveDir)%(FileName)%(Extension)",
                        @"Assets\Scripts\Codes\Model\Generate\ClientServer\**\*.cs Generate\%(RecursiveDir)%(FileName)%(Extension)");
                }

                if (path.EndsWith("Unity.ModelView.csproj"))
                {
                    content = content.Replace("<Compile Include=\"Assets\\Scripts\\Empty\\ModelView\\Empty.cs\" />", string.Empty);
                    content = content.Replace("<None Include=\"Assets\\Scripts\\Empty\\ModelView\\Unity.ModelView.asmdef\" />", string.Empty);
                    content = GenerateCustomProject(path, content,
                        @"Assets\Scripts\Codes\ModelView\**\*.cs %(RecursiveDir)%(FileName)%(Extension)");
                }
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
                link.InnerText = linkStr;
                //compile.AppendChild(link);
                compile.SetAttribute("Include", p);
                itemGroup.AppendChild(compile);
            }

            var projectReference = newDoc.CreateElement("ProjectReference", newDoc.DocumentElement.NamespaceURI);
            projectReference.SetAttribute("Include", @"..\Share\Analyzer\Share.Analyzer.csproj");
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