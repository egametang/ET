using HybridCLR.Editor.AOT;
using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Commands
{
    using Analyzer = HybridCLR.Editor.AOT.Analyzer;
    public static class AOTReferenceGeneratorCommand
    {

        [MenuItem("HybridCLR/Generate/AOTGenericReference", priority = 102)]
        public static void CompileAndGenerateAOTGenericReference()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(target);
            GenerateAOTGenericReference(target);
        }

        /// <summary>
        /// 计算热更代码中的泛型引用
        /// </summary>
        /// <param name="target"></param>
        public static void GenerateAOTGenericReference(BuildTarget target)
        {
            var gs = SettingsUtil.HybridCLRSettings;
            List<string> hotUpdateDllNames = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;

            AssemblyReferenceDeepCollector collector = new AssemblyReferenceDeepCollector(MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(target, hotUpdateDllNames), hotUpdateDllNames);
            var analyzer = new Analyzer(new Analyzer.Options
            {
                MaxIterationCount = Math.Min(20, gs.maxGenericReferenceIteration),
                Collector = collector,
            });

            analyzer.Run();

            var writer = new GenericReferenceWriter();
            writer.Write(analyzer.AotGenericTypes.ToList(), analyzer.AotGenericMethods.ToList(), $"{Application.dataPath}/{gs.outputAOTGenericReferenceFile}");
            AssetDatabase.Refresh();
        }



        //[MenuItem("HybridCLR/Generate/AOTGenericReference2", priority = 103)]
        //public static void GeneratedAOTGenericReferenceExcludeExists()
        //{
        //    GeneratedAOTGenericReferenceExcludeExists(EditorUserBuildSettings.activeBuildTarget);
        //}

        /// <summary>
        /// 计算热更新代码中的泛型引用，但排除AOT已经存在的泛型引用
        /// </summary>
        /// <param name="target"></param>
        /// 
        public static void GeneratedAOTGenericReferenceExcludeExistsAOTClassAndMethods(BuildTarget target)
        {

            var gs = SettingsUtil.HybridCLRSettings;
            List<string> hotUpdateDllNames = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;

            AssemblyReferenceDeepCollector hotUpdateCollector = new AssemblyReferenceDeepCollector(MetaUtil.CreateHotUpdateAndAOTAssemblyResolver(target, hotUpdateDllNames), hotUpdateDllNames);
            var hotUpdateAnalyzer = new Analyzer(new Analyzer.Options
            {
                MaxIterationCount = Math.Min(10, gs.maxGenericReferenceIteration),
                Collector = hotUpdateCollector,
            });

            hotUpdateAnalyzer.Run();


            string aotDllDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(target);
            List<string> aotAssemblyNames = Directory.Exists(aotDllDir) ?
                Directory.GetFiles(aotDllDir, "*.dll", SearchOption.TopDirectoryOnly).Select(Path.GetFileNameWithoutExtension).ToList()
                : new List<string>();
            if (aotAssemblyNames.Count == 0)
            {
                throw new Exception($"no aot assembly found. please run `HybridCLR/Generate/All` or `HybridCLR/Generate/AotDlls` to generate aot dlls before runing `HybridCLR/Generate/AOTGenericReference`");
            }
            AssemblyReferenceDeepCollector aotCollector = new AssemblyReferenceDeepCollector(MetaUtil.CreateAOTAssemblyResolver(target), aotAssemblyNames);
            var aotAnalyzer = new Analyzer(new Analyzer.Options
            {
                MaxIterationCount = Math.Min(10, gs.maxGenericReferenceIteration),
                Collector = aotCollector,
                ComputeAotAssembly = true,
            });

            aotAnalyzer.Run();

            var (resultTypes, resultMethods) = ExcludeExistAOTGenericTypeAndMethodss(hotUpdateAnalyzer.AotGenericTypes.ToList(), hotUpdateAnalyzer.AotGenericMethods.ToList(), aotAnalyzer.AotGenericTypes.ToList(), aotAnalyzer.AotGenericMethods.ToList());
            var writer = new GenericReferenceWriter();
            writer.Write(resultTypes, resultMethods, $"{Application.dataPath}/{gs.outputAOTGenericReferenceFile}");
            AssetDatabase.Refresh();
        }


        private static (List<GenericClass>, List<GenericMethod>) ExcludeExistAOTGenericTypeAndMethodss(List<GenericClass> hotUpdateTypes, List<GenericMethod> hotUpdateMethods, List<GenericClass> aotTypes, List<GenericMethod> aotMethods)
        {
            var types = new List<GenericClass>();

            var typeSig2Type = hotUpdateTypes.ToDictionary(t => t.Type.DefinitionAssembly.Name + ":" + t.ToTypeSig(), t => t);
            foreach (var t in aotTypes)
            {
                string key = t.Type.DefinitionAssembly.Name + ":" + t.ToTypeSig();
                if (typeSig2Type.TryGetValue(key, out var removedType))
                {
                    typeSig2Type.Remove(key);
                    Debug.Log($"remove AOT type:{removedType.ToTypeSig()} ");
                }
            }

            var methodSig2Method = hotUpdateMethods.ToDictionary(m => m.Method.DeclaringType.DefinitionAssembly.Name + ":" + m.ToMethodSpec().ToString(), m => m);
            foreach (var m in aotMethods)
            {
                string key = m.Method.DeclaringType.DefinitionAssembly.Name + ":" + m.ToMethodSpec().ToString();
                if (methodSig2Method.TryGetValue(key, out var removedMethod))
                {
                    methodSig2Method.Remove(key);
                    Debug.Log($"remove AOT method:{removedMethod.ToMethodSpec()} ");
                }
            }

            return (typeSig2Type.Values.ToList(), methodSig2Method.Values.ToList());
        }
    }
}
