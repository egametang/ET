using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    [Serializable]
    public class ReportSummary
    {
        /// <summary>
        /// YooAsset版本
        /// </summary>
        public string YooVersion;

        /// <summary>
        /// 引擎版本
        /// </summary>
        public string UnityVersion;

        /// <summary>
        /// 构建时间
        /// </summary>
        public string BuildDate;

        /// <summary>
        /// 构建耗时（单位：秒）
        /// </summary>
        public int BuildSeconds;

        /// <summary>
        /// 构建平台
        /// </summary>
        public BuildTarget BuildTarget;

        /// <summary>
        /// 构建模式
        /// </summary>
        public EBuildMode BuildMode;

        /// <summary>
        /// 构建管线
        /// </summary>
        public string BuildPipeline;

        /// <summary>
        /// 构建包裹名称
        /// </summary>
        public string BuildPackageName;

        /// <summary>
        /// 构建包裹版本
        /// </summary>
        public string BuildPackageVersion;

        // 收集器配置
        public bool UniqueBundleName;
        public bool EnableAddressable;
        public bool LocationToLower;
        public bool IncludeAssetGUID;
        public bool IgnoreDefaultType;
        public bool AutoCollectShaders;

        // 构建参数
        public bool EnableSharePackRule;
        public string EncryptionClassName;
        public EFileNameStyle FileNameStyle;
        public ECompressOption CompressOption;
        public bool DisableWriteTypeTree;
        public bool IgnoreTypeTreeChanges;

        // 构建结果
        public int AssetFileTotalCount;
        public int MainAssetTotalCount;
        public int AllBundleTotalCount;
        public long AllBundleTotalSize;
        public int EncryptedBundleTotalCount;
        public long EncryptedBundleTotalSize;
    }
}