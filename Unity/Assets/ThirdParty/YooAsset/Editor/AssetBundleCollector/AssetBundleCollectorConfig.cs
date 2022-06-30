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
		public const string ConfigVersion = "1.0";

		public const string XmlVersion = "Version";
		public const string XmlCommon = "Common";
		public const string XmlEnableAddressable = "AutoAddressable";
		public const string XmlAutoCollectShader = "AutoCollectShader";
		public const string XmlShaderBundleName = "ShaderBundleName";
		public const string XmlGroup = "Group";
		public const string XmlGroupName = "GroupName";
		public const string XmlGroupDesc = "GroupDesc";
		public const string XmlCollector = "Collector";
		public const string XmlCollectPath = "CollectPath";
		public const string XmlCollectorType = "CollectType";
		public const string XmlAddressRule = "AddressRule";
		public const string XmlPackRule = "PackRule";
		public const string XmlFilterRule = "FilterRule";
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
			XmlDocument xml = new XmlDocument();
			xml.Load(filePath);
			XmlElement root = xml.DocumentElement;

			// 读取配置版本
			string configVersion = root.GetAttribute(XmlVersion);
			if(configVersion != ConfigVersion)
			{
				throw new Exception($"The config version is invalid : {configVersion}");
			}

			// 读取公共配置
			bool enableAddressable = false;
			bool autoCollectShaders = false;
			string shaderBundleName = string.Empty;
			var commonNodeList = root.GetElementsByTagName(XmlCommon);
			if (commonNodeList.Count > 0)
			{
				XmlElement commonElement = commonNodeList[0] as XmlElement;
				if (commonElement.HasAttribute(XmlEnableAddressable) == false)
					throw new Exception($"Not found attribute {XmlEnableAddressable} in {XmlCommon}");
				if (commonElement.HasAttribute(XmlAutoCollectShader) == false)
					throw new Exception($"Not found attribute {XmlAutoCollectShader} in {XmlCommon}");
				if (commonElement.HasAttribute(XmlShaderBundleName) == false)
					throw new Exception($"Not found attribute {XmlShaderBundleName} in {XmlCommon}");

				enableAddressable = commonElement.GetAttribute(XmlEnableAddressable) == "True" ? true : false;
				autoCollectShaders = commonElement.GetAttribute(XmlAutoCollectShader) == "True" ? true : false;
				shaderBundleName = commonElement.GetAttribute(XmlShaderBundleName);
			}

			// 读取分组配置
			List<AssetBundleCollectorGroup> groupTemper = new List<AssetBundleCollectorGroup>();
			var groupNodeList = root.GetElementsByTagName(XmlGroup);
			foreach (var groupNode in groupNodeList)
			{
				XmlElement groupElement = groupNode as XmlElement;
				if (groupElement.HasAttribute(XmlGroupName) == false)
					throw new Exception($"Not found attribute {XmlGroupName} in {XmlGroup}");
				if (groupElement.HasAttribute(XmlGroupDesc) == false)
					throw new Exception($"Not found attribute {XmlGroupDesc} in {XmlGroup}");
				if (groupElement.HasAttribute(XmlAssetTags) == false)
					throw new Exception($"Not found attribute {XmlAssetTags} in {XmlGroup}");

				AssetBundleCollectorGroup group = new AssetBundleCollectorGroup();
				group.GroupName = groupElement.GetAttribute(XmlGroupName);
				group.GroupDesc = groupElement.GetAttribute(XmlGroupDesc);
				group.AssetTags = groupElement.GetAttribute(XmlAssetTags);
				groupTemper.Add(group);

				// 读取收集器配置
				var collectorNodeList = groupElement.GetElementsByTagName(XmlCollector);
				foreach (var collectorNode in collectorNodeList)
				{
					XmlElement collectorElement = collectorNode as XmlElement;
					if (collectorElement.HasAttribute(XmlCollectPath) == false)
						throw new Exception($"Not found attribute {XmlCollectPath} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlCollectorType) == false)
						throw new Exception($"Not found attribute {XmlCollectorType} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlAddressRule) == false)
						throw new Exception($"Not found attribute {XmlAddressRule} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlPackRule) == false)
						throw new Exception($"Not found attribute {XmlPackRule} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlFilterRule) == false)
						throw new Exception($"Not found attribute {XmlFilterRule} in {XmlCollector}");
					if (collectorElement.HasAttribute(XmlAssetTags) == false)
						throw new Exception($"Not found attribute {XmlAssetTags} in {XmlCollector}");

					AssetBundleCollector collector = new AssetBundleCollector();
					collector.CollectPath = collectorElement.GetAttribute(XmlCollectPath);
					collector.CollectorType = StringUtility.NameToEnum<ECollectorType>(collectorElement.GetAttribute(XmlCollectorType));
					collector.AddressRuleName = collectorElement.GetAttribute(XmlAddressRule);
					collector.PackRuleName = collectorElement.GetAttribute(XmlPackRule);
					collector.FilterRuleName = collectorElement.GetAttribute(XmlFilterRule);
					collector.AssetTags = collectorElement.GetAttribute(XmlAssetTags); ;
					group.Collectors.Add(collector);
				}
			}

			// 保存配置数据
			AssetBundleCollectorSettingData.ClearAll();
			AssetBundleCollectorSettingData.Setting.EnableAddressable = enableAddressable;
			AssetBundleCollectorSettingData.Setting.AutoCollectShaders = autoCollectShaders;
			AssetBundleCollectorSettingData.Setting.ShadersBundleName = shaderBundleName;
			AssetBundleCollectorSettingData.Setting.Groups.AddRange(groupTemper);
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
			commonElement.SetAttribute(XmlAutoCollectShader, AssetBundleCollectorSettingData.Setting.AutoCollectShaders.ToString());
			commonElement.SetAttribute(XmlShaderBundleName, AssetBundleCollectorSettingData.Setting.ShadersBundleName);
			root.AppendChild(commonElement);

			// 设置分组配置
			foreach (var group in AssetBundleCollectorSettingData.Setting.Groups)
			{
				var groupElement = xmlDoc.CreateElement(XmlGroup);
				groupElement.SetAttribute(XmlGroupName, group.GroupName);
				groupElement.SetAttribute(XmlGroupDesc, group.GroupDesc);
				groupElement.SetAttribute(XmlAssetTags, group.AssetTags);
				root.AppendChild(groupElement);

				// 设置收集器配置
				foreach (var collector in group.Collectors)
				{
					var collectorElement = xmlDoc.CreateElement(XmlCollector);
					collectorElement.SetAttribute(XmlCollectPath, collector.CollectPath);
					collectorElement.SetAttribute(XmlCollectorType, collector.CollectorType.ToString());
					collectorElement.SetAttribute(XmlAddressRule, collector.AddressRuleName);
					collectorElement.SetAttribute(XmlPackRule, collector.PackRuleName);
					collectorElement.SetAttribute(XmlFilterRule, collector.FilterRuleName);
					collectorElement.SetAttribute(XmlAssetTags, collector.AssetTags);
					groupElement.AppendChild(collectorElement);
				}
			}

			// 生成配置文件
			xmlDoc.Save(savePath);
			Debug.Log($"导出配置完毕！");
		}
	}
}