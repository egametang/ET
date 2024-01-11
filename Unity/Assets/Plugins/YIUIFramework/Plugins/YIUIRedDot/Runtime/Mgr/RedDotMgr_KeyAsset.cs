using System.Collections.Generic;
using ET;
using UnityEngine;

namespace YIUIFramework
{
    public partial class RedDotMgr
    {
        private const string RedDotKeyAssetName = "RedDotKeyAsset";

        private Dictionary<ERedDotKeyType, RedDotKeyData> m_AllRedDotKeyData =
            new Dictionary<ERedDotKeyType, RedDotKeyData>();

        public IReadOnlyDictionary<ERedDotKeyType, RedDotKeyData> AllRedDotKeyData => m_AllRedDotKeyData;

        private RedDotKeyAsset m_RedDotKeyAsset;

        /// <summary>
        /// 加载key
        /// </summary>
        private async ETTask<bool> LoadKeyAsset()
        {
            m_RedDotKeyAsset = await YIUILoadHelper.LoadAssetAsync<RedDotKeyAsset>(RedDotKeyAssetName);

            if (m_RedDotKeyAsset == null)
            {
                Debug.LogError($"初始化失败 没有加载到目标数据 {RedDotKeyAssetName}");
                return false;
            }

            InitKeyData();
            YIUILoadHelper.Release(m_RedDotKeyAsset);
            return true;
        }

        /// <summary>
        /// 初始化Key相关
        /// </summary>
        private void InitKeyData()
        {
            m_AllRedDotKeyData.Clear();

            foreach (var keyData in m_RedDotKeyAsset.AllRedDotDic.Values)
            {
                m_AllRedDotKeyData.Add((ERedDotKeyType)keyData.Id, keyData);
            }
        }

        /// <summary>
        /// 获取key描述
        /// </summary>
        public string GetKeyDes(ERedDotKeyType keyType)
        {
            if (!m_AllRedDotKeyData.ContainsKey(keyType))
            {
                Debug.LogError($"不存在这个key {keyType}");
                return "";
            }

            var keyData = m_AllRedDotKeyData[keyType];
            return keyData.Des;
        }
    }
}