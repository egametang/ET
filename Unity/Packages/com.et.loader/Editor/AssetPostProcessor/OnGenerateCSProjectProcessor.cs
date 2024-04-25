using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public class OnGenerateCSProjectProcessor: AssetPostprocessor
    {
        /// <summary>
        /// 对生成的C#项目文件(.csproj)进行处理
        /// 文档:https://learn.microsoft.com/zh-cn/visualstudio/gamedev/unity/extensibility/customize-project-files-created-by-vstu#%E6%A6%82%E8%A7%88
        /// </summary>
        public static string OnGeneratedCSProject(string path, string content)
        {
            GlobalConfig globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
            // 判空原因:初次打开工程时会加载失败, 因为此时Unity的资源数据库(AssetDatabase)还未完成初始化
            BuildType buildType = globalConfig != null? globalConfig.BuildType : BuildType.Release;
            if (buildType == BuildType.Release)
            {
                content = content.Replace("<Optimize>false</Optimize>", "<Optimize>true</Optimize>");
                content = content.Replace(";DEBUG;", ";");
            }

            if (path.EndsWith("Unity.Core.csproj"))
            {
                return GenerateCustomProject(content, globalConfig.CodeMode);
            }

            if (path.EndsWith("Unity.Model.csproj"))
            {
                return GenerateCustomProject(content, globalConfig.CodeMode, "Model");
            }
            
            if (path.EndsWith("Unity.Hotfix.csproj"))
            {
                return GenerateCustomProject(content, globalConfig.CodeMode, "Hotfix");
            }

            if (path.EndsWith("Unity.ModelView.csproj"))
            {
                return GenerateCustomProject(content, globalConfig.CodeMode, "ModelView");
            }
            if (path.EndsWith("Unity.HotfixView.csproj"))
            {
                return GenerateCustomProject(content, globalConfig.CodeMode, "HotfixView");
            }

            return content;
        }

        /// <summary>
        /// 对生成的解决方案文件(.sln)进行处理, 此处主要为了隐藏一些没有作用的C#项目
        /// </summary>
        public static string OnGeneratedSlnSolution(string _, string content)
        {
            // Client
            content = HideCSProject(content, "Ignore.Generate.Client.csproj");
            content = HideCSProject(content, "Ignore.Model.Client.csproj");
            content = HideCSProject(content, "Ignore.Hotfix.Client.csproj");
            content = HideCSProject(content, "Ignore.ModelView.Client.csproj");
            content = HideCSProject(content, "Ignore.HotfixView.Client.csproj");

            // Server
            content = HideCSProject(content, "Ignore.Generate.Server.csproj");
            content = HideCSProject(content, "Ignore.Model.Server.csproj");
            content = HideCSProject(content, "Ignore.Hotfix.Server.csproj");

            // ClientServer
            content = HideCSProject(content, "Ignore.Generate.ClientServer.csproj");

            return content;
        }

        /// <summary>
        /// 自定义C#项目配置
        /// 参考链接:
        /// https://zhuanlan.zhihu.com/p/509046784
        /// https://learn.microsoft.com/zh-cn/visualstudio/ide/reference/build-events-page-project-designer-csharp?view=vs-2022
        /// https://learn.microsoft.com/zh-cn/visualstudio/ide/how-to-specify-build-events-csharp?view=vs-2022
        /// </summary>
        static string GenerateCustomProject(string content, CodeMode codeMode, string dllName = "")
        {
            XmlDocument doc = new();
            doc.LoadXml(content);
            var newDoc = doc.Clone() as XmlDocument;
            var rootNode = newDoc.GetElementsByTagName("Project")[0];

            {
                string links = "<Compile Include=\"Library/PackageCache/com.et.*/Runtime~/" + dllName + "~/Share/**/*.cs\">\n            " +
                    "<Link>$([System.String]::new(%(RecursiveDir)).Substring(7, $([System.String]::new(%(RecursiveDir)).Replace(\"Runtime~\", \"\")\"%(FileName)%(Extension)</Link>\n" +
                    "</Compile>\n\n        " +
                    "<Compile Include=\"Library/PackageCache/com.et.*/Runtime~/" + dllName + "/Client/**/*.cs\">\n            " +
                    "<Link>%(RecursiveDir)%(FileName)%(Extension)</Link>\n" +
                    "</Compile>\n\n        " +
                    "<Compile Include=\"Library/PackageCache/com.et.*/Runtime~/" + dllName + "/Server/**/*.cs\">\n            " +
                    "<Link>%(RecursiveDir)%(FileName)%(Extension)</Link>\n" +
                    "</Compile>\n\n        " +
                    "<Compile Include=\"Packages/com.et.*/Runtime~/" + dllName + "/Share/**/*.cs\">\n            " +
                    "<Link>%(RecursiveDir)%(FileName)%(Extension)</Link>\n" +
                    "</Compile>\n\n        " +
                    "<Compile Include=\"Packages/com.et.*/Runtime~/" + dllName + "/Client/**/*.cs\">\n            " +
                    "<Link>%(RecursiveDir)%(FileName)%(Extension)</Link>\n" +
                    "</Compile>\n\n        " +
                    "<Compile Include=\"Packages/com.et.*/Runtime~/" + dllName + "/Server/**/*.cs\">\n            " +
                    "<Link>%(RecursiveDir)%(FileName)%(Extension)</Link>\n" +
                    "</Compile>";
                
                switch (dllName)
                {
                    case "Model":
                        links += "<Compile Include=\"../Generate/*/" + codeMode + "/**/*.cs\"><Link>Generate/%(RecursiveDir)%(FileName)%(Extension)</Link></Compile>";
                        links += "<Compile Include=\"Packages/com.et.*/Runtime~/CodeMode/" + codeMode + "/**/*.cs\"><Link>%(RecursiveDir)%(FileName)%(Extension)</Link></Compile>";
                        break;
                    case "Hotfix":
                        break;
                    case "HotfixView":
                        break;
                    case "ModelView":
                        break;
                    default:
                        links = null;
                        break;
                }
                
                if (links != null)
                {
                    XmlElement itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
                    itemGroup.InnerXml = links;
                    rootNode.AppendChild(itemGroup);
                }
            }

            // 添加分析器引用
            {
                XmlElement itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
                var projectReference = newDoc.CreateElement("ProjectReference", newDoc.DocumentElement.NamespaceURI);
                projectReference.SetAttribute("Include", @"..\Share\Analyzer\Share.Analyzer.csproj");
                projectReference.SetAttribute("OutputItemType", @"Analyzer");
                projectReference.SetAttribute("ReferenceOutputAssembly", @"false");

                var project = newDoc.CreateElement("Project", newDoc.DocumentElement.NamespaceURI);
                project.InnerText = @"{d1f2986b-b296-4a2d-8f12-be9f470014c3}";
                projectReference.AppendChild(project);

                var name = newDoc.CreateElement("Name", newDoc.DocumentElement.NamespaceURI);
                name.InnerText = "Analyzer";
                projectReference.AppendChild(name);

                itemGroup.AppendChild(projectReference);
                rootNode.AppendChild(itemGroup);
            }

            // AfterBuild(字符串替换后作用是编译后复制到CodeDir)
            {

                string afterBuild =
                        $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/$(TargetName).dll.bytes\" ContinueOnError=\"false\" />\n" +
                        $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.CodeDir}/$(TargetName).pdb.bytes\" ContinueOnError=\"false\" />\n" +
                        $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).dll\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/$(TargetName).dll\" ContinueOnError=\"false\" />\n" +
                        $"    <Copy SourceFiles=\"$(TargetDir)/$(TargetName).pdb\" DestinationFiles=\"$(ProjectDir)/{Define.BuildOutputDir}/$(TargetName).pdb\" ContinueOnError=\"false\" />\n";
                switch (dllName)
                {
                    case "Model":
                    case "Hotfix":
                    case "HotfixView":
                    case "ModelView":
                    {
                        var target = newDoc.CreateElement("Target", newDoc.DocumentElement.NamespaceURI);
                        target.SetAttribute("Name", "AfterBuild");
                        target.InnerXml = afterBuild;
                        rootNode.AppendChild(target);
                        break;
                    }
                }
            }

            using StringWriter sw = new();
            using XmlTextWriter tx = new(sw);
            tx.Formatting = Formatting.Indented;
            newDoc.WriteTo(tx);
            tx.Flush();
            return sw.GetStringBuilder().ToString();
        }

        /// <summary>
        /// 隐藏指定项目
        /// </summary>
        static string HideCSProject(string content, string projectName)
        {
            return Regex.Replace(content, $"Project.*{projectName}.*\nEndProject", string.Empty);
        }
    }
}