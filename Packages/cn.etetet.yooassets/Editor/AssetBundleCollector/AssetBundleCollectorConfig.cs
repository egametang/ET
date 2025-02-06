using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class AssetBundleCollectorConfig
    {
        public const string ConfigVersion = "v2.0.0";

        public const string XmlVersion = "Version";
        public const string XmlCommon = "Common";

        public const string XmlShowPackageView = "ShowPackageView";
        public const string XmlShowEditorAlias = "ShowEditorAlias";
        public const string XmlUniqueBundleName = "UniqueBundleName";

        public const string XmlPackage = "Package";
        public const string XmlPackageName = "PackageName";
        public const string XmlPackageDesc = "PackageDesc";
        public const string XmlEnableAddressable = "AutoAddressable";
        public const string XmlLocationToLower = "LocationToLower";
        public const string XmlIncludeAssetGUID = "IncludeAssetGUID";
        public const string XmlIgnoreDefaultType = "IgnoreDefaultType";

        public const string XmlGroup = "Group";
        public const string XmlGroupActiveRule = "GroupActiveRule";
        public const string XmlGroupName = "GroupName";
        public const string XmlGroupDesc = "GroupDesc";

        public const string XmlCollector = "Collector";
        public const string XmlCollectPath = "CollectPath";
        public const string XmlCollectorGUID = "CollectGUID";
        public const string XmlCollectorType = "CollectType";
        public const string XmlAddressRule = "AddressRule";
        public const string XmlPackRule = "PackRule";
        public const string XmlFilterRule = "FilterRule";
        public const string XmlUserData = "UserData";
        public const string XmlAssetTags = "AssetTags";

        /// <summary>
        /// 导入XML配置表
        /// </summary>
        public static void ImportXmlConfig(string filePath)
        {
            if (File.Exists(filePath) == false)
                throw new FileNotFoundException(filePath);

            if (Path.GetExtension(filePath) != ".xml")
                throw new Exception($"Only support xml : {filePath}");

            // 加载配置文件
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            XmlElement root = xmlDoc.DocumentElement;

            // 读取配置版本
            string configVersion = root.GetAttribute(XmlVersion);
            if (configVersion != ConfigVersion)
            {
                if (UpdateXmlConfig(xmlDoc) == false)
                    throw new Exception($"The config version update failed : {configVersion} -> {ConfigVersion}");
                else
                    Debug.Log($"The config version update succeed : {configVersion} -> {ConfigVersion}");
            }

            // 读取公共配置
            bool uniqueBundleName = false;
            bool showPackageView = false;
            bool showEditorAlias = false;
            var commonNodeList = root.GetElementsByTagName(XmlCommon);
            if (commonNodeList.Count > 0)
            {
                XmlElement commonElement = commonNodeList[0] as XmlElement;
                if (commonElement.HasAttribute(XmlShowPackageView))
                    showPackageView = commonElement.GetAttribute(XmlShowPackageView) == "True" ? true : false;
                if (commonElement.HasAttribute(XmlShowEditorAlias))
                    showEditorAlias = commonElement.GetAttribute(XmlShowEditorAlias) == "True" ? true : false;
                if (commonElement.HasAttribute(XmlUniqueBundleName))
                    uniqueBundleName = commonElement.GetAttribute(XmlUniqueBundleName) == "True" ? true : false;
            }

            // 读取包裹配置
            List<AssetBundleCollectorPackage> packages = new List<AssetBundleCollectorPackage>();
            var packageNodeList = root.GetElementsByTagName(XmlPackage);
            foreach (var packageNode in packageNodeList)
            {
                XmlElement packageElement = packageNode as XmlElement;
                if (packageElement.HasAttribute(XmlPackageName) == false)
                    throw new Exception($"Not found attribute {XmlPackageName} in {XmlPackage}");
                if (packageElement.HasAttribute(XmlPackageDesc) == false)
                    throw new Exception($"Not found attribute {XmlPackageDesc} in {XmlPackage}");

                AssetBundleCollectorPackage package = new AssetBundleCollectorPackage();
                package.PackageName = packageElement.GetAttribute(XmlPackageName);
                package.PackageDesc = packageElement.GetAttribute(XmlPackageDesc);
                package.EnableAddressable = packageElement.GetAttribute(XmlEnableAddressable) == "True" ? true : false;
                package.LocationToLower = packageElement.GetAttribute(XmlLocationToLower) == "True" ? true : false;
                package.IncludeAssetGUID = packageElement.GetAttribute(XmlIncludeAssetGUID) == "True" ? true : false;
                package.IgnoreDefaultType = packageElement.GetAttribute(XmlIgnoreDefaultType) == "True" ? true : false;
                packages.Add(package);

                // 读取分组配置
                var groupNodeList = packageElement.GetElementsByTagName(XmlGroup);
                foreach (var groupNode in groupNodeList)
                {
                    XmlElement groupElement = groupNode as XmlElement;
                    if (groupElement.HasAttribute(XmlGroupActiveRule) == false)
                        throw new Exception($"Not found attribute {XmlGroupActiveRule} in {XmlGroup}");
                    if (groupElement.HasAttribute(XmlGroupName) == false)
                        throw new Exception($"Not found attribute {XmlGroupName} in {XmlGroup}");
                    if (groupElement.HasAttribute(XmlGroupDesc) == false)
                        throw new Exception($"Not found attribute {XmlGroupDesc} in {XmlGroup}");
                    if (groupElement.HasAttribute(XmlAssetTags) == false)
                        throw new Exception($"Not found attribute {XmlAssetTags} in {XmlGroup}");

                    AssetBundleCollectorGroup group = new AssetBundleCollectorGroup();
                    group.ActiveRuleName = groupElement.GetAttribute(XmlGroupActiveRule);
                    group.GroupName = groupElement.GetAttribute(XmlGroupName);
                    group.GroupDesc = groupElement.GetAttribute(XmlGroupDesc);
                    group.AssetTags = groupElement.GetAttribute(XmlAssetTags);
                    package.Groups.Add(group);

                    // 读取收集器配置
                    var collectorNodeList = groupElement.GetElementsByTagName(XmlCollector);
                    foreach (var collectorNode in collectorNodeList)
                    {
                        XmlElement collectorElement = collectorNode as XmlElement;
                        if (collectorElement.HasAttribute(XmlCollectPath) == false)
                            throw new Exception($"Not found attribute {XmlCollectPath} in {XmlCollector}");
                        if (collectorElement.HasAttribute(XmlCollectorGUID) == false)
                            throw new Exception($"Not found attribute {XmlCollectorGUID} in {XmlCollector}");
                        if (collectorElement.HasAttribute(XmlCollectorType) == false)
                            throw new Exception($"Not found attribute {XmlCollectorType} in {XmlCollector}");
                        if (collectorElement.HasAttribute(XmlAddressRule) == false)
                            throw new Exception($"Not found attribute {XmlAddressRule} in {XmlCollector}");
                        if (collectorElement.HasAttribute(XmlPackRule) == false)
                            throw new Exception($"Not found attribute {XmlPackRule} in {XmlCollector}");
                        if (collectorElement.HasAttribute(XmlFilterRule) == false)
                            throw new Exception($"Not found attribute {XmlFilterRule} in {XmlCollector}");
                        if (collectorElement.HasAttribute(XmlUserData) == false)
                            throw new Exception($"Not found attribute {XmlUserData} in {XmlCollector}");
                        if (collectorElement.HasAttribute(XmlAssetTags) == false)
                            throw new Exception($"Not found attribute {XmlAssetTags} in {XmlCollector}");

                        AssetBundleCollector collector = new AssetBundleCollector();
                        collector.CollectPath = collectorElement.GetAttribute(XmlCollectPath);
                        collector.CollectorGUID = collectorElement.GetAttribute(XmlCollectorGUID);
                        collector.CollectorType = EditorTools.NameToEnum<ECollectorType>(collectorElement.GetAttribute(XmlCollectorType));
                        collector.AddressRuleName = collectorElement.GetAttribute(XmlAddressRule);
                        collector.PackRuleName = collectorElement.GetAttribute(XmlPackRule);
                        collector.FilterRuleName = collectorElement.GetAttribute(XmlFilterRule);
                        collector.UserData = collectorElement.GetAttribute(XmlUserData);
                        collector.AssetTags = collectorElement.GetAttribute(XmlAssetTags);
                        group.Collectors.Add(collector);
                    }
                }
            }

            // 检测配置错误
            foreach (var package in packages)
            {
                package.CheckConfigError();
            }

            // 保存配置数据
            AssetBundleCollectorSettingData.ClearAll();
            AssetBundleCollectorSettingData.Setting.ShowPackageView = showPackageView;
            AssetBundleCollectorSettingData.Setting.ShowEditorAlias = showEditorAlias;
            AssetBundleCollectorSettingData.Setting.UniqueBundleName = uniqueBundleName;
            AssetBundleCollectorSettingData.Setting.Packages.AddRange(packages);
            AssetBundleCollectorSettingData.SaveFile();
            Debug.Log($"Asset bundle collector config import complete！");
        }

        /// <summary>
        /// 导出XML配置表
        /// </summary>
        public static void ExportXmlConfig(string savePath)
        {
            if (File.Exists(savePath))
                File.Delete(savePath);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine("</root>");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sb.ToString());
            XmlElement root = xmlDoc.DocumentElement;

            // 设置配置版本
            root.SetAttribute(XmlVersion, ConfigVersion);

            // 设置公共配置
            var commonElement = xmlDoc.CreateElement(XmlCommon);
            commonElement.SetAttribute(XmlShowPackageView, AssetBundleCollectorSettingData.Setting.ShowPackageView.ToString());
            commonElement.SetAttribute(XmlShowEditorAlias, AssetBundleCollectorSettingData.Setting.ShowEditorAlias.ToString());
            commonElement.SetAttribute(XmlUniqueBundleName, AssetBundleCollectorSettingData.Setting.UniqueBundleName.ToString());
            root.AppendChild(commonElement);

            // 设置Package配置
            foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
            {
                var packageElement = xmlDoc.CreateElement(XmlPackage);
                packageElement.SetAttribute(XmlPackageName, package.PackageName);
                packageElement.SetAttribute(XmlPackageDesc, package.PackageDesc);
                packageElement.SetAttribute(XmlEnableAddressable, package.EnableAddressable.ToString());
                packageElement.SetAttribute(XmlLocationToLower, package.LocationToLower.ToString());
                packageElement.SetAttribute(XmlIncludeAssetGUID, package.IncludeAssetGUID.ToString());
                packageElement.SetAttribute(XmlIgnoreDefaultType, package.IgnoreDefaultType.ToString());
                root.AppendChild(packageElement);

                // 设置分组配置
                foreach (var group in package.Groups)
                {
                    var groupElement = xmlDoc.CreateElement(XmlGroup);
                    groupElement.SetAttribute(XmlGroupActiveRule, group.ActiveRuleName);
                    groupElement.SetAttribute(XmlGroupName, group.GroupName);
                    groupElement.SetAttribute(XmlGroupDesc, group.GroupDesc);
                    groupElement.SetAttribute(XmlAssetTags, group.AssetTags);
                    packageElement.AppendChild(groupElement);

                    // 设置收集器配置
                    foreach (var collector in group.Collectors)
                    {
                        var collectorElement = xmlDoc.CreateElement(XmlCollector);
                        collectorElement.SetAttribute(XmlCollectPath, collector.CollectPath);
                        collectorElement.SetAttribute(XmlCollectorGUID, collector.CollectorGUID);
                        collectorElement.SetAttribute(XmlCollectorType, collector.CollectorType.ToString());
                        collectorElement.SetAttribute(XmlAddressRule, collector.AddressRuleName);
                        collectorElement.SetAttribute(XmlPackRule, collector.PackRuleName);
                        collectorElement.SetAttribute(XmlFilterRule, collector.FilterRuleName);
                        collectorElement.SetAttribute(XmlUserData, collector.UserData);
                        collectorElement.SetAttribute(XmlAssetTags, collector.AssetTags);
                        groupElement.AppendChild(collectorElement);
                    }
                }
            }

            // 生成配置文件
            xmlDoc.Save(savePath);
            Debug.Log($"Asset bundle collector config export complete！");
        }

        /// <summary>
        /// 升级XML配置表
        /// </summary>
        private static bool UpdateXmlConfig(XmlDocument xmlDoc)
        {
            XmlElement root = xmlDoc.DocumentElement;
            string configVersion = root.GetAttribute(XmlVersion);
            if (configVersion == ConfigVersion)
                return true;

            return false;
        }
    }
}