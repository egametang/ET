using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public class OnGenerateCSProjectProcessor: AssetPostprocessor
    {
        /// <summary>
        /// 文档:https://learn.microsoft.com/zh-cn/visualstudio/gamedev/unity/extensibility/customize-project-files-created-by-vstu#%E6%A6%82%E8%A7%88
        /// </summary>
        public static string OnGeneratedCSProject(string path, string content)
        {
            //刷新一下CodeMode，防止某些本地修改CodeMode测试后，使用svn更新，被改名的Ignore.asmdef被还原的问题
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            AssemblyTool.RefreshCodeMode(globalConfig.CodeMode);

            if (globalConfig.BuildType != BuildType.Debug)
            {
                content = content.Replace("<Optimize>false</Optimize>", "<Optimize>true</Optimize>");
                content = content.Replace(";DEBUG;", ";");
            }

            if (path.EndsWith("Unity.Core.csproj"))
            {
                content = GenerateCustomProject(content);
            }

            if (path.EndsWith("Unity.ModelView.csproj"))
            {
                content = GenerateCustomProject(content);
                content = AddCopyAfterBuild(content);
            }

            if (path.EndsWith("Unity.HotfixView.csproj"))
            {
                content = GenerateCustomProject(content);
                content = AddCopyAfterBuild(content);
            }

            if (path.EndsWith("Unity.Model.csproj"))
            {
                content = GenerateCustomProject(content);
                content = AddCopyAfterBuild(content);
            }

            if (path.EndsWith("Unity.Hotfix.csproj"))
            {
                content = GenerateCustomProject(content);
                content = AddCopyAfterBuild(content);
            }

            return content;
        }

        /// <summary>
        /// 编译dll文件后额外复制的目录配置
        /// </summary>
        private static string AddCopyAfterBuild(string content)
        {
            content = content.Replace("<Target Name=\"AfterBuild\" />",
                "<Target Name=\"PostBuild\" AfterTargets=\"PostBuildEvent\">\n" +
                $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/$(TargetName).dll.bytes\" ContinueOnError=\"false\" />\n" +
                $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/$(TargetName).pdb.bytes\" ContinueOnError=\"false\" />\n" +
                $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/$(TargetName).dll\" ContinueOnError=\"false\" />\n" +
                $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/$(TargetName).pdb\" ContinueOnError=\"false\" />\n" +
                "  </Target>\n");
            return content;
        }

        /// <summary>
        /// 自定义C#项目配置
        /// 参考链接:
        /// https://zhuanlan.zhihu.com/p/509046784
        /// https://learn.microsoft.com/zh-cn/visualstudio/ide/reference/build-events-page-project-designer-csharp?view=vs-2022
        /// https://learn.microsoft.com/zh-cn/visualstudio/ide/how-to-specify-build-events-csharp?view=vs-2022
        /// </summary>
        private static string GenerateCustomProject(string content, params string[] links)
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
                compile.AppendChild(link);
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

