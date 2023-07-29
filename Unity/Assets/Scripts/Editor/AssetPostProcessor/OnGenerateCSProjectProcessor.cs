using System;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;

namespace ET
{
    public class OnGenerateCSProjectProcessor: AssetPostprocessor
    {
        private static string[] deleteFile = new string[] 
        { 
            "Library/ScriptAssemblies/Unity.AllModel.dll", 
            "Library/ScriptAssemblies/Unity.AllHotfix.dll",
            "Library/ScriptAssemblies/Unity.AllModel.pdb", 
            "Library/ScriptAssemblies/Unity.AllHotfix.pdb", 
        };
        
        public static string OnGeneratedCSProject(string path, string content)
        {
            if (path.EndsWith("Unity.Core.csproj"))
            {
                return GenerateCustomProject(path, content);
            }
            
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");

            if (path.EndsWith("Unity.AllModel.csproj"))
            {
                if (!Define.EnableDll)
                {
                    return content;
                }
                if (globalConfig.BuildType == BuildType.Release)
                {
                    content = content.Replace("<Optimize>false</Optimize>", "<Optimize>true</Optimize>");
                    content = content.Replace(";DEBUG;", ";");
                }

                string[] files = Array.Empty<string>();
                switch (globalConfig.CodeMode)
                {
                    case CodeMode.Client:
                        files = new[]
                        {
                            @"Assets\Scripts\Model\Client\**\*.cs Model\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Model\Share\**\*.cs Model\Share\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Model\Generate\Client\**\*.cs Model\Generate\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\ModelView\Client\**\*.cs ModelView\Client\%(RecursiveDir)%(FileName)%(Extension)",
                        };
                        break;
                    case CodeMode.ClientServer:
                        files = new[]
                        {
                            @"Assets\Scripts\Model\Server\**\*.cs Model\Server\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Model\Client\**\*.cs Model\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Model\Share\**\*.cs Model\Share\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Model\Generate\ClientServer\**\*.cs Model\Generate\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\ModelView\Client\**\*.cs ModelView\Client\%(RecursiveDir)%(FileName)%(Extension)",
                        };
                        break;
                }

                content = GenerateCustomProject(path, content, files);
                content = content.Replace("<Target Name=\"AfterBuild\" />",
                    "   <Target Name=\"PostBuild\" AfterTargets=\"PostBuildEvent\">\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/Model.dll.bytes\" ContinueOnError=\"false\" />\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/Model.pdb.bytes\" ContinueOnError=\"false\" />\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/Model.dll\" ContinueOnError=\"false\" />\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/Model.pdb\" ContinueOnError=\"false\" />\n" +
                    "   </Target>\n");
                return content;
            }
            
            if (path.EndsWith("Unity.AllHotfix.csproj"))
            {
                if (!Define.EnableDll)
                {
                    return content;
                }
                
                if (globalConfig.BuildType == BuildType.Release)
                {
                    content = content.Replace("<Optimize>false</Optimize>", "<Optimize>true</Optimize>");
                    content = content.Replace(";DEBUG;", ";");
                }

                string[] files = Array.Empty<string>();
                switch (globalConfig.CodeMode)
                {
                    case CodeMode.Client:
                        files = new[]
                        {
                            @"Assets\Scripts\Hotfix\Client\**\*.cs Hotfix\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Hotfix\Share\**\*.cs Hotfix\Share\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\HotfixView\Client\**\*.cs HotfixView\Client\%(RecursiveDir)%(FileName)%(Extension)"
                        };
                        break;
                    case CodeMode.ClientServer:
                        files = new[]
                        {
                            @"Assets\Scripts\Hotfix\Client\**\*.cs Hotfix\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Hotfix\Server\**\*.cs Hotfix\Server\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Hotfix\Share\**\*.cs Hotfix\Share\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\HotfixView\Client\**\*.cs HotfixView\Client\%(RecursiveDir)%(FileName)%(Extension)"
                        };
                        break;
                }

                content = GenerateCustomProject(path, content, files);
                content = content.Replace("<Target Name=\"AfterBuild\" />",
                    "   <Target Name=\"PostBuild\" AfterTargets=\"PostBuildEvent\">\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/Hotfix.dll.bytes\" ContinueOnError=\"false\" />\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/Hotfix.pdb.bytes\" ContinueOnError=\"false\" />\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/Hotfix.dll\" ContinueOnError=\"false\" />\n" +
                    $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/Hotfix.pdb\" ContinueOnError=\"false\" />\n" +
                    "   </Target>\n");
            }


            foreach (string file in deleteFile)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);    
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

            var target = newDoc.CreateElement("Target", newDoc.DocumentElement.NamespaceURI);
            target.SetAttribute("Name", "AfterBuild");
            rootNode.AppendChild(target);

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

            using StringWriter sw = new();
            using XmlTextWriter tx = new(sw);
            tx.Formatting = Formatting.Indented;
            newDoc.WriteTo(tx);
            tx.Flush();
            return sw.GetStringBuilder().ToString();
        }
    }
}