using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor
{
    public static partial class BuildConfig
    {

        /// <summary>
        /// 所有热更新dll列表。放到此列表中的dll在打包时OnFilterAssemblies回调中被过滤。
        /// </summary>
        public static List<string> HotUpdateAssemblies { get; } = new List<string>
        {
        };

        public static List<string> AOTMetaAssemblies { get; } = new List<string>()
        {
            "mscorlib.dll",
            "System.dll",
            "System.Core.dll", // 如果使用了Linq，需要这个
            "Unity.Core.dll",
            "Unity.Mono.dll",
            "Unity.ThirdParty.dll",
            "MongoDB.Bson.dll",
            "CommandLine.dll",
            "NLog.dll",

            //
            // 注意！修改这个列表请同步修改HotFix2模块中App.cs文件中的 LoadMetadataForAOTAssembly函数中aotDllList列表。
            // 两者需要完全一致
            //
        };

        public static List<string> AssetBundleFiles { get; } = new List<string>
        {
            //"common",
        };
    }
}
