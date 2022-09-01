using HybridCLR.Editor.Generators;
using HybridCLR.Editor.Generators.MethodBridge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor
{
    internal class MethodBridgeHelper
    {

        private static void CleanIl2CppBuildCache()
        {
            string il2cppBuildCachePath = BuildConfig.Il2CppBuildCacheDir;
            if (!Directory.Exists(il2cppBuildCachePath))
            {
                return;
            }
            Debug.Log($"clean il2cpp build cache:{il2cppBuildCachePath}");
            Directory.Delete(il2cppBuildCachePath, true);
        }

        private static List<Assembly> CollectDependentAssemblies(Dictionary<string, Assembly> allAssByName, List<Assembly> dlls)
        {
            for(int i = 0; i < dlls.Count; i++)
            {
                Assembly ass = dlls[i];
                foreach (var depAssName in ass.GetReferencedAssemblies())
                {
                    if (!allAssByName.ContainsKey(depAssName.Name))
                    {
                        Debug.Log($"ignore ref assembly:{depAssName.Name}");
                        continue;
                    }
                    Assembly depAss = allAssByName[depAssName.Name];
                    if (!dlls.Contains(depAss))
                    {
                        dlls.Add(depAss);
                    }
                }
            }
            return dlls;
        }

        private static List<Assembly> GetScanAssembiles()
        {
            var allAssByName = new Dictionary<string, Assembly>();
            foreach(var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                allAssByName[ass.GetName().Name] = ass;
            }
            //CompileDllHelper.CompileDllActiveBuildTarget();

            var rootAssemblies = BuildConfig.HotUpdateAssemblies
                .Select(dll => Path.GetFileNameWithoutExtension(dll)).Concat(GeneratorConfig.GetExtraAssembiles())
                .Where(name => allAssByName.ContainsKey(name)).Select(name => allAssByName[name]).ToList();
            //var rootAssemblies = GeneratorConfig.GetExtraAssembiles()
            //    .Where(name => allAssByName.ContainsKey(name)).Select(name => allAssByName[name]).ToList();
            CollectDependentAssemblies(allAssByName, rootAssemblies);
            rootAssemblies.Sort((a, b) => a.GetName().Name.CompareTo(b.GetName().Name));
            Debug.Log($"assembly count:{rootAssemblies.Count}");
            foreach(var ass in rootAssemblies)
            {
                //Debug.Log($"scan assembly:{ass.GetName().Name}");
            }
            return rootAssemblies;
        }

        private static void GenerateMethodBridgeCppFile(PlatformABI platform, string fileName, bool optimized)
        {
            string outputFile = $"{BuildConfig.MethodBridgeCppDir}/{fileName}.cpp";
            var g = new MethodBridgeGenerator(new MethodBridgeGeneratorOptions()
            {
                CallConvention = platform,
                HotfixAssemblies = BuildConfig.HotUpdateAssemblies.Select(name =>
                    AppDomain.CurrentDomain.GetAssemblies().First(ass => ass.GetName().Name + ".dll" == name)).ToList(),
                AllAssemblies = optimized ? GetScanAssembiles() : AppDomain.CurrentDomain.GetAssemblies().ToList(),
                OutputFile = outputFile,
                Optimized = optimized,
            });

            g.PrepareMethods();
            g.Generate();
            Debug.LogFormat("== output:{0} ==", outputFile);
            CleanIl2CppBuildCache();
        }

        //[MenuItem("HybridCLR/MethodBridge/Arm64")]
        //public static void MethodBridge_Arm64()
        //{
        //    GenerateMethodBridgeCppFile(PlatformABI.Arm64, "MethodBridge_Arm64");
        //}

        //[MenuItem("HybridCLR/MethodBridge/Universal64")]
        //public static void MethodBridge_Universal64()
        //{
        //    GenerateMethodBridgeCppFile(PlatformABI.Universal64, "MethodBridge_Universal64");
        //}

        //[MenuItem("HybridCLR/MethodBridge/Universal32")]
        //public static void MethodBridge_Universal32()
        //{
        //    GenerateMethodBridgeCppFile(PlatformABI.Universal32, "MethodBridge_Universal32");
        //}

        public static void GenerateMethodBridgeAll(bool optimized)
        {
            GenerateMethodBridgeCppFile(PlatformABI.Arm64, "MethodBridge_Arm64", optimized);
            GenerateMethodBridgeCppFile(PlatformABI.Universal64, "MethodBridge_Universal64", optimized);
            GenerateMethodBridgeCppFile(PlatformABI.Universal32, "MethodBridge_Universal32", optimized);
        }

        [MenuItem("HybridCLR/MethodBridge/All_高度精简")]
        public static void MethodBridge_All_Optimized()
        {
            GenerateMethodBridgeAll(true);
        }

        [MenuItem("HybridCLR/MethodBridge/All_完整(新手及开发期推荐)")]
        public static void MethodBridge_All_Normal()
        {
            GenerateMethodBridgeAll(false);
        }
    }
}
