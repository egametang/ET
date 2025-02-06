using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    [Serializable]
    public class ReportIndependAsset
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 资源GUID
        /// </summary>
        public string AssetGUID;

        /// <summary>
        /// 资源类型
        /// </summary>
        public string AssetType;

        /// <summary>
        /// 资源文件大小
        /// </summary>
        public long FileSize;
    }
}