//------------------------------------------------------------
// Author: 亦亦
// Mail: 379338943@qq.com
// Data: 2023年2月12日
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using ET;
using UnityEngine;

namespace YIUIFramework
{
    /// <summary>
    /// 红点 管理器
    /// </summary>
    public partial class RedDotMgr: MgrSingleton<RedDotMgr>, IManagerAsyncInit
    {
        private const bool SyncSetCount = false; //实时修改红点还是异步脏标定时修改

        private const string RedDotConfigAssetName = "RedDotConfigAsset";

        private Dictionary<ERedDotKeyType, RedDotData> m_AllRedDotData = new Dictionary<ERedDotKeyType, RedDotData>();

        public IReadOnlyDictionary<ERedDotKeyType, RedDotData> AllRedDotData => m_AllRedDotData;

        private RedDotConfigAsset m_RedDotConfigAsset;

        protected override void OnDispose()
        {
            DisposeDirty();
        }

        protected override async ETTask<bool> MgrAsyncInit()
        {
            var resultConfig = await LoadConfigAsset();
            if (!resultConfig) return false;
            #if UNITY_EDITOR || YIUIMACRO_REDDOT_STACK
            var resultKey = await LoadKeyAsset();
            if (!resultKey) return false;
            #endif

            if (!SyncSetCount)
            {
                InitAsyncDirty();
            }

            return true;
        }

        /// <summary>
        /// 加载config
        /// </summary>
        private async ETTask<bool> LoadConfigAsset()
        {
            m_RedDotConfigAsset = await YIUILoadHelper.LoadAssetAsync<RedDotConfigAsset>(RedDotConfigAssetName);

            if (m_RedDotConfigAsset == null)
            {
                Debug.LogError($"初始化失败 没有加载到目标数据 {RedDotConfigAssetName}");
                return false;
            }

            InitNewAllData();
            InitLinkData();
            YIUILoadHelper.Release(m_RedDotConfigAsset);
            return true;
        }

        /// <summary>
        /// 初始化创建所有数据
        /// </summary>
        private void InitNewAllData()
        {
            m_AllRedDotData.Clear();

            foreach (ERedDotKeyType key in Enum.GetValues(typeof (ERedDotKeyType)))
            {
                //有配置则使用配置 没有则使用默认配置
                var config = m_RedDotConfigAsset.GetConfigData(key) ?? new RedDotConfigData { Key = key };
                var data   = new RedDotData(config);
                m_AllRedDotData.Add(key, data);
            }
        }

        /// <summary>
        /// 初始化红点关联数据
        /// </summary>
        private void InitLinkData()
        {
            foreach (var config in m_RedDotConfigAsset.AllRedDotConfigDic.Values)
            {
                var data = GetData(config.Key);
                if (data == null) continue;
                foreach (var parentId in config.ParentList)
                {
                    var parentData = GetData(parentId);
                    if (parentData != null)
                    {
                        data.AddParent(parentData);
                    }
                }
            }
        }
    }
}