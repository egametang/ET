using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET.PackageManager.Editor
{
    [Serializable]
    public class PackageLastVersionData
    {
        [SerializeField]
        public string Name;

        [SerializeField]
        public string Version;
    }

    public class PackageInfoAsset : ScriptableObject
    {
        [SerializeField]
        public PackageLastVersionData[] AllLastPackageInfo; //所有包信息的最新版本号

        [SerializeField]
        public string[] BanPackageInfo; //黑名单 可不请求数据

        [SerializeField]
        public long LastUpdateTime; //上一次更新时间 防止重复更新 如果想强制更新可以手动设置改为0即可

        [SerializeField]
        public long UpdateInterval = 3600; //更新间隔 真不需要很高的频率 哪里有插件一直更新的

        [NonSerialized]
        public Dictionary<string, string> AllLastPackageInfoDic = new();

        [NonSerialized]
        public HashSet<string> BanPackageInfoHash = new();

        public void ReUpdateInfo()
        {
            AllLastPackageInfoDic.Clear();
            if (AllLastPackageInfo != null)
            {
                foreach (var info in AllLastPackageInfo)
                {
                    if (info == null) continue;
                    var key   = info.Name;
                    var value = info.Version;
                    AllLastPackageInfoDic[key] = value;
                }
            }

            BanPackageInfoHash.Clear();
            if (BanPackageInfo != null)
            {
                foreach (var info in BanPackageInfo)
                {
                    BanPackageInfoHash.Add(info);
                }
            }
        }

        public void ReSetAllLastPackageInfo()
        {
            AllLastPackageInfo = null;
            AllLastPackageInfoDic.Clear();
        }
    }
}
