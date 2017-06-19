using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Model;
using UnityEditor;

namespace MyEditor
{
	public class BehaviorBatchOperation
	{
		public delegate void ExcuteTreeOperate(BehaviorTreeConfig treeConfig, string treePath);

		public static Dictionary<int, BehaviorTreeConfig> mId2TreeDict = new Dictionary<int, BehaviorTreeConfig>();
		public static Dictionary<string, ClientNodeTypeProto> mCientNodeDict;
		public static Dictionary<string, bool> mUseNodeDict = new Dictionary<string, bool>();

		public static void HasUseNode(NodeProto node, string treePath)
		{
			if (string.IsNullOrEmpty(node.name))
			{
				Log.Error($"node name can not be empty!! {treePath}");
			}
			if (!mUseNodeDict.ContainsKey(node.name))
			{
				Log.Warning($"{node.name} not exist!!!");
			}
			else
			{
				mUseNodeDict[node.name] = true;
			}
			for (int i = 0; i < node.children.Count; i++)
			{
				HasUseNode(node.children[i], treePath);
			}
		}

		public static void SaveOneTree(BehaviorTreeConfig treeConfig, string treePath, params object[] paramList)
		{
			EditorUtility.SetDirty(treeConfig.gameObject);
			AssetDatabase.SaveAssets();
		}

		public static void CheckHasName(BehaviorTreeConfig config, string path, string nodeName)
		{
			if (HasNode(config.RootNodeProto, nodeName))
			{
				Log.Info($"{nodeName}: {path}");
			}
		}

		public static bool HasNode(NodeProto node, string name)
		{
			if (node.name == name)
			{
				return true;
			}
			for (int i = 0; i < node.children.Count; i++)
			{
				if (HasNode(node.children[i], name))
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasNodeField(NodeProto node, Type searchType, string prefabPath)
		{
			FieldInfo[] fieldInfos = ExportNodeTypeConfig.GetFieldInfos(node.name);
			foreach (FieldInfo fieldInfo in fieldInfos)
			{
				if (fieldInfo.FieldType == searchType)
				{
					Log.Info($"{prefabPath}");
					return true;
				}
			}
			for (int i = 0; i < node.children.Count; i++)
			{
				if (HasNodeField(node.children[i], searchType, prefabPath))
				{
					return true;
				}
			}
			return false;
		}
	}
}