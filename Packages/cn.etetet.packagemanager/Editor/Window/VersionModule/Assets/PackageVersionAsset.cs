#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace ET.PackageManager.Editor
{
    [HideLabel]
    public class PackageVersionAsset : SerializedScriptableObject
    {
        [OdinSerialize]
        [ReadOnly]
        [LabelText("所有包版本数据")]
        public Dictionary<string, PackageVersionData> AllPackageVersionData = new();

        [NonSerialized]
        [OdinSerialize]
        [LabelText("上一次更新时间")]
        public long LastUpdateTime; //上一次更新时间 防止重复更新 如果想强制更新可以手动设置改为0即可

        [NonSerialized]
        [OdinSerialize]
        [LabelText("更新间隔")]
        public long UpdateInterval = 3600; //更新间隔 真不需要很高的频率 哪里有插件一直更新的
    }
}
#endif
