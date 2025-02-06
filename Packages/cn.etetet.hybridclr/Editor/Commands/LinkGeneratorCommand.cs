using HybridCLR.Editor.Link;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Commands
{
    using Analyzer = HybridCLR.Editor.Link.Analyzer;

    public static class LinkGeneratorCommand
    {

        [MenuItem("HybridCLR/Generate/LinkXml", priority = 100)]
        public static void GenerateLinkXml()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(target);
            GenerateLinkXml(target);
        }

        public static void GenerateLinkXml(BuildTarget target)
        {
            var ls = SettingsUtil.HybridCLRSettings;

            List<string> hotfixAssemblies = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;

            var analyzer = new Analyzer(MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(target, hotfixAssemblies));
            var refTypes = analyzer.CollectRefs(hotfixAssemblies);

            Debug.Log($"[LinkGeneratorCommand] hotfix assembly count:{hotfixAssemblies.Count}, ref type count:{refTypes.Count} output:{Application.dataPath}/{ls.outputLinkFile}");
            var linkXmlWriter = new LinkXmlWriter();
            linkXmlWriter.Write($"{Application.dataPath}/{ls.outputLinkFile}", refTypes);
            AssetDatabase.Refresh();
        }
    }
}
