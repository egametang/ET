using HybridCLR.Editor.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace HybridCLR.Editor.BuildProcessors
{
    /// <summary>
    /// 将热更新dll从Build过程中过滤，防止打包到主工程中
    /// </summary>
    internal class FilterHotFixAssemblies : IFilterBuildAssemblies
    {
        public int callbackOrder => 0;

        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            if (!SettingsUtil.Enable)
            {
                Debug.Log($"[FilterHotFixAssemblies] disabled");
                return assemblies;
            }
            List<string> allHotUpdateDllNames = SettingsUtil.HotUpdateAssemblyNamesExcludePreserved;

            // 检查是否重复填写
            var hotUpdateDllSet = new HashSet<string>();
            foreach(var hotUpdateDll in allHotUpdateDllNames)
            {
                if (string.IsNullOrWhiteSpace(hotUpdateDll))
                {
                    throw new BuildFailedException($"hot update assembly name cann't be empty");
                }
                if (!hotUpdateDllSet.Add(hotUpdateDll))
                {
                    throw new BuildFailedException($"hot update assembly:{hotUpdateDll} is duplicated");
                }
            }

            var assResolver = MetaUtil.CreateHotUpdateAssemblyResolver(EditorUserBuildSettings.activeBuildTarget, allHotUpdateDllNames);
            // 检查是否填写了正确的dll名称
            foreach (var hotUpdateDllName in allHotUpdateDllNames)
            {
                if (assemblies.Select(Path.GetFileNameWithoutExtension).All(ass => ass != hotUpdateDllName) 
                    && string.IsNullOrEmpty(assResolver.ResolveAssembly(hotUpdateDllName, false)))
                {
                    throw new BuildFailedException($"hot update assembly:{hotUpdateDllName} doesn't exist");
                }
            }

            // 将热更dll从打包列表中移除
            return assemblies.Where(ass =>
            {
                string assName = Path.GetFileNameWithoutExtension(ass);
                bool reserved = allHotUpdateDllNames.All(dll => !assName.Equals(dll, StringComparison.Ordinal));
                if (!reserved)
                {
                    Debug.Log($"[FilterHotFixAssemblies] filter assembly:{assName}");
                }
                return reserved;
            }).ToArray();
        }
    }
}
