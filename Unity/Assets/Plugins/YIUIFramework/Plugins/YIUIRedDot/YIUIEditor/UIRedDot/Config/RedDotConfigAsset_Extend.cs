#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace YIUIFramework.Editor
{
    public static class RedDotConfigAsset_Extend
    {
        internal static void SetAllRedDotConfigList(this RedDotConfigAsset self, List<RedDotConfigData> dataList)
        {
            self.m_AllRedDotConfigDic.Clear();
            foreach (var data in dataList)
            {
                self.m_AllRedDotConfigDic.Add(data.Key, data);
            }

            EditorUtility.SetDirty(self);
        }

        internal static bool ContainsKey(this RedDotConfigAsset self, ERedDotKeyType key)
        {
            return self.m_AllRedDotConfigDic.ContainsKey(key);
        }

        internal static bool RemoveConfigData(this RedDotConfigAsset self, ERedDotKeyType key)
        {
            var result = self.m_AllRedDotConfigDic.Remove(key);
            EditorUtility.SetDirty(self);
            return result;
        }

        internal static bool AddConfigData(this RedDotConfigAsset self, RedDotConfigData data)
        {
            if (self.m_AllRedDotConfigDic.ContainsKey(data.Key))
            {
                Debug.LogError($"已存在 无法重复添加 {data.Key}");
                return false;
            }

            self.m_AllRedDotConfigDic.Add(data.Key, data);
            EditorUtility.SetDirty(self);
            return true;
        }
    }
}
#endif