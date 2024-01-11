#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

namespace YIUIFramework.Editor
{
    internal static class UIRedDotKeyEditorHelper
    {
        internal static RedDotLinkData GetCheckKeyLink(RedDotConfigAsset configAsset, ERedDotKeyType key)
        {
            var linkData = new RedDotLinkData
            {
                Key = key
            };

            foreach (var data in configAsset.AllRedDotConfigDic.Values)
            {
                if (data.Key == key)
                {
                    linkData.ConfigSet = true;
                    continue;
                }

                foreach (var parent in data.ParentList)
                {
                    if (parent == key)
                    {
                        linkData.LinkKey.Add(data.Key);
                        break;
                    }
                }
            }

            return linkData;
        }

        internal static void RemoveKeyByLink(RedDotConfigAsset configAsset, RedDotLinkData linkData)
        {
            if (linkData.ConfigSet)
            {
                configAsset.RemoveConfigData(linkData.Key);
            }

            foreach (var linkKey in linkData.LinkKey)
            {
                var configData = configAsset.GetConfigData(linkKey);
                if (configData == null)
                {
                    Debug.LogError($"这里不应该获取到一个未知的结果  请检查 {linkKey}");
                    continue;
                }

                configData.ParentList.Remove(linkData.Key);
            }
        }

        internal static void ChangeKeyByLink(RedDotConfigAsset configAsset, RedDotLinkData linkData,
                                             RedDotKeyData     changeKeyData)
        {
            var newKey = (ERedDotKeyType)changeKeyData.Id;
            if (linkData.ConfigSet)
            {
                var configData = configAsset.GetConfigData(linkData.Key);
                if (configData == null)
                {
                    Debug.LogError($"这里不应该获取到一个未知的结果  请检查 {linkData.Key}");
                    return;
                }

                //移除老的
                configAsset.RemoveConfigData(linkData.Key);

                //添加一个新的
                var newConfigData = new RedDotConfigData
                {
                    Key        = newKey,
                    SwitchTips = configData.SwitchTips,
                    ParentList = configData.ParentList
                };
                configAsset.AddConfigData(newConfigData);
            }

            foreach (var linkKey in linkData.LinkKey)
            {
                var configData = configAsset.GetConfigData(linkKey);
                if (configData == null)
                {
                    Debug.LogError($"这里不应该获取到一个未知的结果  请检查 {linkKey}");
                    continue;
                }

                configData.ParentList.Remove(linkData.Key);
                configData.ParentList.Add(newKey);
            }
        }
    }
}
#endif