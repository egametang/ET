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
		public const string ConfigVersion = "2.4";

		public const string XmlVersion = "Version";
		public const string XmlCommon = "Common";
		public const string XmlEnableAddressable = "AutoAddressable";
		public const string XmlUniqueBundleName = "UniqueBundleName";
		public const string XmlShowPackageView = "ShowPackageView";
		public const string XmlShowEditorAlias = "ShowEditorAlias";

		public const string XmlPackage = "Package";
		public const string XmlPackageName = "PackageName";
		public const string XmlPackageDesc = "PackageDesc";

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
			bool enableAddressable = false;
			bool uniqueBundleName = false;
			bool showPackageView = false;
			bool showEditorAlias = false;
			var commonNodeList = root.GetElementsByTagName(XmlCommon);
			if (commonNodeList.Count > 0)
			{
				XmlElement commonElement = commonNodeList[0] as XmlElement;
				if (commonElement.HasAttribute(XmlEnableAddressable) == false)
					throw new Exception($"Not found attribute {XmlEnableAddressable} in {XmlCommon}");
				if (commonElement.HasAttribute(XmlUniqueBundleName) == false)
					throw new Exception($"Not found attribute {XmlUniqueBundleName} in {XmlCommon}");
				if (commonElement.HasAttribute(XmlShowPackageView) == false)
					throw new Exception($"Not found attribute {XmlShowPackageView} in {XmlCommon}");
				if (commonElement.HasAttribute(XmlShowEditorAlias) == false)
					throw new Exception($"Not found attribute {XmlShowEditorAlias} in {XmlCommon}");

				enableAddressable = commonElement.GetAttribute(XmlEnableAddressable) == "True" ? true : false;
				uniqueBundleName = commonElement.GetAttribute(XmlUniqueBundleName) == "True" ? true : false;
				showPackageView = commonElement.GetAttribute(XmlShowPackageView) == "True" ? true : false;
				showEditorAlias = commonElement.GetAttribute(XmlShowEditorAlias) == "True" ? true : false;
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
			foreach(var package in packages)
			{
				package.CheckConfigError();
			}

			// 保存配置数据
			AssetBundleCollectorSettingData.ClearAll();
			AssetBundleCollectorSettingData.Setting.EnableAddressable = enableAddressable;
			AssetBundleCollectorSettingData.Setting.UniqueBundleName = uniqueBundleName;
			AssetBundleCollectorSettingData.Setting.ShowPackageView = showPackageView;
			AssetBundleCollectorSettingData.Setting.ShowEditorAlias = showEditorAlias;
			AssetBundleCollectorSettingData.Setting.Packages.AddRange(packages);
			AssetBundleCollectorSettingData.SaveFile();
			Debug.Log($"导入配置完毕！");
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
			commonElement.SetAttribute(XmlEnableAddressable, AssetBundleCollectorSettingData.Setting.EnableAddressable.ToString());
			commonElement.SetAttribute(XmlUniqueBundleName, AssetBundleCollectorSettingData.Setting.UniqueBundleName.ToString());
			commonElement.SetAttribute(XmlShowPackageView, AssetBundleCollectorSettingData.Setting.ShowPackageView.ToString());
			commonElement.SetAttribute(XmlShowEditorAlias, AssetBundleCollectorSettingData.Setting.ShowEditorAlias.ToString());
			root.AppendChild(commonElement);

			// 设置Package配置
			foreach (var package in AssetBundleCollectorSettingData.Setting.Packages)
			{
				var packageElement = xmlDoc.CreateElement(XmlPackage);
				packageElement.SetAttribute(XmlPackageName, package.PackageName);
				packageElement.SetAttribute(XmlPackageDesc, package.PackageDesc);
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
			Debug.Log($"导出配置完毕！");
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

			// 1.0 -> 2.0
			if (configVersion == "1.0")
			{
				// 添加公共元素属性
				var commonNodeList = root.GetElementsByTagName(XmlCommon);
				if (commonNodeList.Count > 0)
				{
					XmlElement commonElement = commonNodeList[0] as XmlElement;
					if (commonElement.HasAttribute(XmlShowPackageView) == false)
						commonElement.SetAttribute(XmlShowPackageView, "False");
				}

				// 添加包裹元素
				var packageElement = xmlDoc.CreateElement(XmlPackage);
				packageElement.SetAttribute(XmlPackageName, "DefaultPackage");
				packageElement.SetAttribute(XmlPackageDesc, string.Empty);
				root.AppendChild(packageElement);

				// 获取所有分组元素
				var groupNodeList = root.GetElementsByTagName(XmlGroup);
				List<XmlElement> temper = new List<XmlElement>(groupNodeList.Count);
				foreach (var groupNode in groupNodeList)
				{
					XmlElement groupElement = groupNode as XmlElement;
					var collectorNodeList = groupElement.GetElementsByTagName(XmlCollector);
					foreach (var collectorNode in collectorNodeList)
					{
						XmlElement collectorElement = collectorNode as XmlElement;
						if (collectorElement.HasAttribute(XmlCollectorGUID) == false)
							collectorElement.SetAttribute(XmlCollectorGUID, string.Empty);
					}
					temper.Add(groupElement);
				}

				// 将分组元素转移至包裹元素下
				foreach (var groupElement in temper)
				{
					root.RemoveChild(groupElement);
					packageElement.AppendChild(groupElement);
				}

				// 更新版本
				root.SetAttribute(XmlVersion, "2.0");
				return UpdateXmlConfig(xmlDoc);
			}

			// 2.0 -> 2.1
			if (configVersion == "2.0")
			{
				// 添加公共元素属性
				var commonNodeList = root.GetElementsByTagName(XmlCommon);
				if (commonNodeList.Count > 0)
				{
					XmlElement commonElement = commonNodeList[0] as XmlElement;
					if (commonElement.HasAttribute(XmlUniqueBundleName) == false)
						commonElement.SetAttribute(XmlUniqueBundleName, "False");
				}

				// 更新版本
				root.SetAttribute(XmlVersion, "2.1");
				return UpdateXmlConfig(xmlDoc);
			}

			// 2.1 -> 2.2
			if (configVersion == "2.1")
			{
				// 添加公共元素属性
				var commonNodeList = root.GetElementsByTagName(XmlCommon);
				if (commonNodeList.Count > 0)
				{
					XmlElement commonElement = commonNodeList[0] as XmlElement;
					if (commonElement.HasAttribute(XmlShowEditorAlias) == false)
						commonElement.SetAttribute(XmlShowEditorAlias, "False");
				}

				// 更新版本
				root.SetAttribute(XmlVersion, "2.2");
				return UpdateXmlConfig(xmlDoc);
			}

			// 2.2 -> 2.3
			if (configVersion == "2.2")
			{
				// 获取所有分组元素
				var groupNodeList = root.GetElementsByTagName(XmlGroup);
				foreach (var groupNode in groupNodeList)
				{
					XmlElement groupElement = groupNode as XmlElement;
					var collectorNodeList = groupElement.GetElementsByTagName(XmlCollector);
					foreach (var collectorNode in collectorNodeList)
					{
						XmlElement collectorElement = collectorNode as XmlElement;
						if (collectorElement.HasAttribute(XmlUserData) == false)
							collectorElement.SetAttribute(XmlUserData, string.Empty);
					}
				}

				// 更新版本
				root.SetAttribute(XmlVersion, "2.3");
				return UpdateXmlConfig(xmlDoc);
			}

			// 2.3 -> 2.4
			if(configVersion == "2.3")
			{
				// 获取所有分组元素
				var groupNodeList = root.GetElementsByTagName(XmlGroup);
				foreach (var groupNode in groupNodeList)
				{
					XmlElement groupElement = groupNode as XmlElement;
					if(groupElement.HasAttribute(XmlGroupActiveRule) == false)
						groupElement.SetAttribute(XmlGroupActiveRule, $"{nameof(EnableGroup)}");
				}

				// 更新版本
				root.SetAttribute(XmlVersion, "2.4");
				return UpdateXmlConfig(xmlDoc);
			}

			return false;
		}
	}
}