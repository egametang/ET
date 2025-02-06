using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    [Serializable]
    public class AssetBundleCollectorGroup
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDesc = string.Empty;

        /// <summary>
        /// 资源分类标签
        /// </summary>
        public string AssetTags = string.Empty;

        /// <summary>
        /// 分组激活规则
        /// </summary>
        public string ActiveRuleName = nameof(EnableGroup);

        /// <summary>
        /// 分组的收集器列表
        /// </summary>
        public List<AssetBundleCollector> Collectors = new List<AssetBundleCollector>();


        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            if (AssetBundleCollectorSettingData.HasActiveRuleName(ActiveRuleName) == false)
                throw new Exception($"Invalid {nameof(IActiveRule)} class type : {ActiveRuleName} in group : {GroupName}");

            // 检测分组是否激活
            IActiveRule activeRule = AssetBundleCollectorSettingData.GetActiveRuleInstance(ActiveRuleName);
            if (activeRule.IsActiveGroup() == false)
                return;

            foreach (var collector in Collectors)
            {
                collector.CheckConfigError();
            }
        }

        /// <summary>
        /// 修复配置错误
        /// </summary>
        public bool FixConfigError()
        {
            bool isFixed = false;
            foreach (var collector in Collectors)
            {
                if (collector.FixConfigError())
                {
                    isFixed = true;
                }
            }
            return isFixed;
        }

        /// <summary>
        /// 获取打包收集的资源文件
        /// </summary>
        public List<CollectAssetInfo> GetAllCollectAssets(CollectCommand command)
        {
            Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(10000);

            // 检测分组是否激活
            IActiveRule activeRule = AssetBundleCollectorSettingData.GetActiveRuleInstance(ActiveRuleName);
            if (activeRule.IsActiveGroup() == false)
            {
                return new List<CollectAssetInfo>();
            }

            // 收集打包资源
            foreach (var collector in Collectors)
            {
                var temper = collector.GetAllCollectAssets(command, this);
                foreach (var collectAsset in temper)
                {
                    if (result.ContainsKey(collectAsset.AssetInfo.AssetPath) == false)
                        result.Add(collectAsset.AssetInfo.AssetPath, collectAsset);
                    else
                        throw new Exception($"The collecting asset file is existed : {collectAsset.AssetInfo.AssetPath} in group : {GroupName}");
                }
            }

            // 检测可寻址地址是否重复
            if (command.EnableAddressable)
            {
                var addressTemper = new Dictionary<string, string>();
                foreach (var collectAssetPair in result)
                {
                    if (collectAssetPair.Value.CollectorType == ECollectorType.MainAssetCollector)
                    {
                        string address = collectAssetPair.Value.Address;
                        string assetPath = collectAssetPair.Value.AssetInfo.AssetPath;
                        if (string.IsNullOrEmpty(address))
                            continue;

                        if (addressTemper.TryGetValue(address, out var existed) == false)
                            addressTemper.Add(address, assetPath);
                        else
                            throw new Exception($"The address is existed : {address} in group : {GroupName} \nAssetPath:\n     {existed}\n     {assetPath}");
                    }
                }
            }

            // 返回列表
            return result.Values.ToList();
        }
    }
}