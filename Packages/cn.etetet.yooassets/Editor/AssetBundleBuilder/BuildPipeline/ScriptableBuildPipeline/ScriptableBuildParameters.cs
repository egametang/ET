using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
    public class ScriptableBuildParameters : BuildParameters
    {
        /// <summary>
        /// 压缩选项
        /// </summary>
        public ECompressOption CompressOption = ECompressOption.Uncompressed;

        /// <summary>
        /// 禁止写入类型树结构（可以降低包体和内存并提高加载效率）
        /// </summary>
        public bool DisableWriteTypeTree = false;

        /// <summary>
        /// 忽略类型树变化
        /// </summary>
        public bool IgnoreTypeTreeChanges = true;


        /// <summary>
        /// 生成代码防裁剪配置
        /// </summary>
        public bool WriteLinkXML = true;

        /// <summary>
        /// 缓存服务器地址
        /// </summary>
        public string CacheServerHost;

        /// <summary>
        /// 缓存服务器端口
        /// </summary>
        public int CacheServerPort;


        /// <summary>
        /// 获取可编程构建管线的构建参数
        /// </summary>
        public BundleBuildParameters GetBundleBuildParameters()
        {
            var targetGroup = UnityEditor.BuildPipeline.GetBuildTargetGroup(BuildTarget);
            var pipelineOutputDirectory = GetPipelineOutputDirectory();
            var buildParams = new BundleBuildParameters(BuildTarget, targetGroup, pipelineOutputDirectory);

            if (CompressOption == ECompressOption.Uncompressed)
                buildParams.BundleCompression = UnityEngine.BuildCompression.Uncompressed;
            else if (CompressOption == ECompressOption.LZMA)
                buildParams.BundleCompression = UnityEngine.BuildCompression.LZMA;
            else if (CompressOption == ECompressOption.LZ4)
                buildParams.BundleCompression = UnityEngine.BuildCompression.LZ4;
            else
                throw new System.NotImplementedException(CompressOption.ToString());

            if (DisableWriteTypeTree)
                buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.DisableWriteTypeTree;

            buildParams.UseCache = true;
            buildParams.CacheServerHost = CacheServerHost;
            buildParams.CacheServerPort = CacheServerPort;
            buildParams.WriteLinkXML = WriteLinkXML;

            return buildParams;
        }
    }
}