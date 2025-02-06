using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    [Serializable]
    internal class DebugPackageData
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 调试数据列表
        /// </summary>
        public List<DebugProviderInfo> ProviderInfos = new List<DebugProviderInfo>(1000);
    }
}