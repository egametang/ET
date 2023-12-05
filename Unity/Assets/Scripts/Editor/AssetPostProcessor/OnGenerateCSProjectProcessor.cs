using System.IO;
using System.Xml;
using UnityEditor;

namespace ET
{
    public class OnGenerateCSProjectProcessor: AssetPostprocessor
    {
        /// <summary>
        /// 文档:https://learn.microsoft.com/zh-cn/visualstudio/gamedev/unity/extensibility/customize-project-files-created-by-vstu#%E6%A6%82%E8%A7%88
        /// </summary>
        public static string OnGeneratedCSProject(string path, string content)
        {
            BuildType buildType = BuildType.Debug;
            CodeMode codeMode = CodeMode.Client;
            GlobalConfig globalConfig = GetGlobalConfig();
            // 初次打开工程时会加载失败, 因为此时Unity的资源数据库(AssetDatabase)还未完成初始化
            if (globalConfig)
            {
                buildType = globalConfig.BuildType;
                codeMode = globalConfig.CodeMode;
            }

            if (buildType == BuildType.Release)
            {
                content = content.Replace("<Optimize>false</Optimize>", "<Optimize>true</Optimize>");
                content = content.Replace(";DEBUG;", ";");
            }

            if (path.EndsWith("Unity.Core.csproj"))
            {
                return GenerateCustomProject(content);
            }

            if (path.EndsWith("Unity.ModelView.csproj"))
            {
                string[] files = new[] { @"Assets\Scripts\Codes\ModelView\Client\**\*.cs ModelView\Client\%(RecursiveDir)%(FileName)%(Extension)", };
                content = GenerateCustomProject(content, files);
                content = AddCopyAfterBuild(content);
            }

            if (path.EndsWith("Unity.HotfixView.csproj"))
            {
                string[] files = new[]
                {
                    @"Assets\Scripts\Codes\HotfixView\Client\**\*.cs HotfixView\Client\%(RecursiveDir)%(FileName)%(Extension)",
                };
                content = GenerateCustomProject(content, files);
                content = AddCopyAfterBuild(content);
            }

            if (path.EndsWith("Unity.Model.csproj"))
            {
                string[] files = { };
                switch (codeMode)
                {
                    case CodeMode.Client:
                        files = new[]
                        {
                            @"Assets\Scripts\Codes\Model\Client\**\*.cs Model\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Model\Share\**\*.cs Model\Share\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Model\Generate\Client\**\*.cs Model\Generate\%(RecursiveDir)%(FileName)%(Extension)",
                        };
                        break;
                    case CodeMode.ClientServer:
                        files = new[]
                        {
                            @"Assets\Scripts\Codes\Model\Server\**\*.cs Model\Server\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Model\Client\**\*.cs Model\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Model\Share\**\*.cs Model\Share\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Model\Generate\ClientServer\**\*.cs Model\Generate\%(RecursiveDir)%(FileName)%(Extension)",
                        };
                        break;
                }

                content = GenerateCustomProject(content, files);
                content = AddCopyAfterBuild(content);
            }

            if (path.EndsWith("Unity.Hotfix.csproj"))
            {
                string[] files = { };
                switch (codeMode)
                {
                    case CodeMode.Client:
                        files = new[]
                        {
                            @"Assets\Scripts\Codes\Hotfix\Client\**\*.cs Hotfix\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Hotfix\Share\**\*.cs Hotfix\Share\%(RecursiveDir)%(FileName)%(Extension)",
                        };
                        break;
                    case CodeMode.ClientServer:
                        files = new[]
                        {
                            @"Assets\Scripts\Codes\Hotfix\Client\**\*.cs Hotfix\Client\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Hotfix\Server\**\*.cs Hotfix\Server\%(RecursiveDir)%(FileName)%(Extension)",
                            @"Assets\Scripts\Codes\Hotfix\Share\**\*.cs Hotfix\Share\%(RecursiveDir)%(FileName)%(Extension)",
                        };
                        break;
                }

                content = GenerateCustomProject(content, files);
                content = AddCopyAfterBuild(content);
            }

            string[] deleteFiles =
            {
                "Library/ScriptAssemblies/Unity.Model.dll", "Library/ScriptAssemblies/Unity.Hotfix.dll",
                "Library/ScriptAssemblies/Unity.Model.pdb", "Library/ScriptAssemblies/Unity.Hotfix.pdb",
                "Library/ScriptAssemblies/Unity.ModelView.dll", "Library/ScriptAssemblies/Unity.HotfixView.dll",
                "Library/ScriptAssemblies/Unity.ModelView.pdb", "Library/ScriptAssemblies/Unity.HotfixView.pdb",
            };

            foreach (string file in deleteFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            return content;
        }

        /// <summary>
        /// 编译dll文件后额外复制的目录配置
        /// </summary>
        private static string AddCopyAfterBuild(string content)
        {
            content = content.Replace("<Target Name=\"AfterBuild\" />",
                "   <Target Name=\"PostBuild\" AfterTargets=\"PostBuildEvent\">\n" +
                $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/$(TargetName).dll.bytes\" ContinueOnError=\"false\" />\n" +
                $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/$(TargetName).pdb.bytes\" ContinueOnError=\"false\" />\n" +
                $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/$(TargetName).dll\" ContinueOnError=\"false\" />\n" +
                $"       <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/$(TargetName).pdb\" ContinueOnError=\"false\" />\n" +
                "   </Target>\n");
            return content;
        }

        /// <summary>
        /// 获取全局配置
        /// </summary>
        private static GlobalConfig GetGlobalConfig()
        {
            return AssetDatabase.LoadAssetAtPath<GlobalConfig>("Assets/Resources/GlobalConfig.asset");
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