using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    [Serializable]
    internal class DebugProviderInfo : IComparer<DebugProviderInfo>, IComparable<DebugProviderInfo>
    {
        /// <summary>
        /// 包裹名
        /// </summary>
        public string PackageName { set; get; }

        /// <summary>
        /// 资源对象路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 资源出生的场景
        /// </summary>
        public string SpawnScene;

        /// <summary>
        /// 资源出生的时间
        /// </summary>
        public string SpawnTime;

        /// <summary>
        /// 加载耗时（单位：毫秒）
        /// </summary>
        public long LoadingTime;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount;

        /// <summary>
        /// 加载状态
        /// </summary>
        public string Status;

        /// <summary>
        /// 依赖的资源包列表
        /// </summary>
        public List<DebugBundleInfo> DependBundleInfos;

        public int CompareTo(DebugProviderInfo other)
        {
            return Compare(this, other);
        }
        public int Compare(DebugProviderInfo a, DebugProviderInfo b)
        {
            return string.CompareOrdinal(a.AssetPath, b.AssetPath);
        }
    }
}