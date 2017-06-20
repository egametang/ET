using System;
using System.Collections.Generic;
using System.Reflection;
using Base;
using Model;
using UnityEditor;
using UnityEngine;
using Component = UnityEngine.Component;
using Object = UnityEngine.Object;

namespace MyEditor
{
	public class BehaviorManager
	{
		public GameObject CurTreeGO { get; set; }
		public BehaviorTreeData CurTree { get; set; }
		public const int NodeIdStartIndex = 100000;
		private int AutoID = NodeIdStartIndex;
		private Dictionary<string, ClientNodeTypeProto> name2NodeProtoDict = new Dictionary<string, ClientNodeTypeProto>(); //节点类型 name索引
		public static List<List<long>> treePathList = new List<List<long>>();
		public string selectNodeName;
		public string selectNodeType;
		public BehaviorTree CurBehaviorTree { get; set; }

		public BehaviorTreeConfig BehaviorTreeConfig
		{
			get
			{
				return CurTreeGO?.GetComponent<BehaviorTreeConfig>();
			}
		}

		public static BehaviorManager Instance { get; } = new BehaviorManager();

		public Dictionary<string, List<ClientNodeTypeProto>> Classify2NodeProtoList { get; private set; } =
			new Dictionary<string, List<ClientNodeTypeProto>>();

		public List<ClientNodeTypeProto> AllNodeProtoList
		{
			get
			{
				List<ClientNodeTypeProto> list = new List<ClientNodeTypeProto>();
				foreach (KeyValuePair<string, List<ClientNodeTypeProto>> item in Instance.Classify2NodeProtoList)
				{
					foreach (ClientNodeTypeProto proto in item.Value)
					{
						list.Add(proto);
					}
				}
				return list;
			}
		}

		//节点配置 get set
		public ClientNodeTypeProto GetNodeTypeProto(string name)
		{
			ClientNodeTypeProto proto = ExportNodeTypeConfig.GetNodeTypeProtoFromDll(name);
			return proto;
		}

		public void FilterClassify()
		{
			this.Classify2NodeProtoList = new Dictionary<string, List<ClientNodeTypeProto>>();
			foreach (ClientNodeTypeProto nodeType in this.name2NodeProtoDict.Values)
			{
				if (nodeType.isDeprecated)
				{
					continue;
				}
				string classify = nodeType.classify;
				if (classify == "")
				{
					classify = "未分类";
				}
				if (!this.Classify2NodeProtoList.ContainsKey(classify))
				{
					this.Classify2NodeProtoList.Add(classify, new List<ClientNodeTypeProto>());
				}
				this.Classify2NodeProtoList[classify].Add(nodeType);
			}
		}

		public int AutoNodeId()
		{
			return ++AutoID;
		}

		public void NewLoadData()
		{
			LoadNodeTypeProto();
			NewLoadPrefabTree();
			FilterClassify();
		}

		public void LoadNodeTypeProto()
		{
			this.name2NodeProtoDict = ExportNodeTypeConfig.ExportToDict();
		}

		public void NewLoadPrefabTree()
		{
			BehaviorTreeConfig config = CurTreeGO.GetComponent<BehaviorTreeConfig>();
			CurTree = BehaviorTreeConfigToTreeData(config);
		}

		public BehaviorTreeData BehaviorTreeConfigToTreeData(BehaviorTreeConfig config)
		{
			BehaviorTreeData tree = new BehaviorTreeData { Root = NodeConfigToNodeData(config.RootNodeConfig) };
			return tree;
		}

		public void printTree(BehaviorNodeData nodeData)
		{
			Log.Info($"printTree  :  {nodeData.Id} {nodeData}");
			foreach (BehaviorNodeData data in nodeData.children)
			{
				printTree(data);
			}
		}

		public bool CheckSatisfyInput()
		{
			NodeProto rootNode = this.BehaviorNodeDataToNodeProto(CurTree.BehaviorNodeData);
			return CheckNodeInput(rootNode);
		}

		public bool CheckNodeInput(NodeProto nodeProto)
		{
			List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.Name, typeof(NodeInputAttribute));
			foreach (NodeFieldDesc desc in list)
			{
				List<string> canInputList = GetCanInPutEnvKeyList(this.NodeProtoToBehaviorNodeData(nodeProto), desc);
				string value = nodeProto.Args.Get(desc.name)?.ToString();
				List<string> resultList = canInputList.FindAll(str => { return str == value; });
				if (resultList.Count == 0)
				{
					Log.Error($"{nodeProto.Name}节点(id:{nodeProto.Id})的{value}输入值非法!");
					return false;
				}
			}
			foreach (NodeProto child in nodeProto.children)
			{
				if (!CheckNodeInput(child))
				{
					return false;
				}
			}
			return true;
		}

		public void SaveAll()
		{
			if (!CheckHasTreeDesc())
			{
				return;
			}
			if (!CheckSatisfyInput())
			{
				return;
			}
			SavePrefabTree();
			Log.Info("保存成功！");
			BehaviorTreeTipsHelper.ShowMessage("保存成功！");
		}

		private void SavePrefabTree()
		{
			ResetTreeId();

			BehaviorTreeConfig config = BehaviorTreeDataToConfig(CurTree);
			RenameTree(config);
			Object.DestroyImmediate(config.gameObject);
		}

		public void ResetTreeId()
		{
			AutoID = NodeIdStartIndex;
			CurTree.Root.ResetId();
		}
		
		public void RemoveUnusedArgs(NodeProto nodeProto)
		{
			ClientNodeTypeProto proto = ExportNodeTypeConfig.GetNodeTypeProtoFromDll(nodeProto.Name);
			List<string> unUsedList = new List<string>();
			foreach (KeyValuePair<string, object> item in nodeProto.Args.Dict())
			{
				if (!proto.new_args_desc.Exists(a => (a.name == item.Key)))
				{
					unUsedList.Add(item.Key);
				}
			}
			foreach (string item in unUsedList)
			{
				nodeProto.Args.Remove(item);
			}
			for (int i = 0; i < nodeProto.children.Count; i++)
			{
				RemoveUnusedArgs(nodeProto.children[i]);
			}
		}

		private bool CheckHasTreeDesc()
		{
			if (string.IsNullOrEmpty(CurTree.Root.Desc))
			{
				Log.Error("行为树描述不可以不填！！！！！！");
				return false;
			}
			return true;
		}

		public void RenameTree(BehaviorTreeConfig config)
		{
			string newName = $"{config.RootNodeConfig.describe}";
			if (!config.RootNodeConfig.describe.StartsWith("BT_"))
			{
				newName = $"BT_{config.RootNodeConfig.describe}";
			}
			config.gameObject.name = newName;
			CurTreeGO = PrefabUtility.ReplacePrefab(config.gameObject, CurTreeGO, ReplacePrefabOptions.ReplaceNameBased);
			string prefabPath = AssetDatabase.GetAssetPath(CurTreeGO);
			string result = AssetDatabase.RenameAsset(prefabPath, newName);
			if (result.Contains("Invalid file name"))
			{
				Log.Error(result);
			}
			EditorUtility.SetDirty(config.gameObject);
		}

		public BehaviorTreeConfig BehaviorTreeDataToConfig(BehaviorTreeData tree)
		{
			GameObject curTreeGo = Object.Instantiate(CurTreeGO);
			BehaviorTreeConfig config = curTreeGo.GetComponent<BehaviorTreeConfig>();
			if (config == null)
			{
				config = curTreeGo.AddComponent<BehaviorTreeConfig>();
			}
			foreach (Transform child in config.gameObject.transform)
			{
				Object.DestroyImmediate(child.gameObject);
			}
			try
			{
				config.RootNodeConfig = this.BehaviorNodeDataToNodeConfig(tree.Root);
			}
			catch (Exception e)
			{
				Log.Error($"tree name : {tree.BehaviorNodeData.Name} {e}");
			}

			config.RootNodeConfig.gameObject.transform.parent = config.gameObject.transform;
			return config;
		}

		public BehaviorNodeData NodeConfigToNodeData(BehaviorNodeConfig nodeProto)
		{
			BehaviorNodeData nodeData = new BehaviorNodeData();
			nodeData.Id = nodeProto.id;
			nodeData.Name = nodeProto.name;
			nodeData.Desc = nodeProto.describe;
			nodeData.Args = nodeProto.GetArgsDict();
			nodeData.children = new List<BehaviorNodeData>();
			//             foreach (var item in nodeData.Args)
			//             {
			//                 Log.Info($"key :{item.Key} value :{item.Value}");
			//             }
			foreach (Transform child in nodeProto.gameObject.transform)
			{
				BehaviorNodeConfig nodeConfig = child.gameObject.GetComponent<BehaviorNodeConfig>();
				BehaviorNodeData childData = NodeConfigToNodeData(nodeConfig);
				nodeData.children.Add(childData);
			}
			return nodeData;
		}

		public BehaviorNodeConfig BehaviorNodeDataToNodeConfig(BehaviorNodeData nodeData)
		{
			GameObject go = new GameObject();
			BehaviorNodeConfig nodeConfig = go.AddComponent<BehaviorNodeConfig>();
			nodeConfig.id = nodeData.Id;
			nodeConfig.name = nodeData.Name;
			go.name = nodeData.Name;
			nodeConfig.describe = nodeData.Desc;
			List<string> unUseList = new List<string>();
			foreach (KeyValuePair<string, object> args in nodeData.Args.Dict())
			{
				if (!ExportNodeTypeConfig.NodeHasField(nodeData.Name, args.Key))
				{
					unUseList.Add(args.Key);
					continue;
				}
				Type originType = ExportNodeTypeConfig.GetFieldType(nodeData.Name, args.Key);
				try
				{
					string fieldName = args.Key;
					object fieldValue = args.Value;
					Type type = BTTypeManager.GetBTType(originType);
					Component comp = go.AddComponent(type);
					FieldInfo fieldNameInfo = type.GetField("fieldName");
					fieldNameInfo.SetValue(comp, fieldName);
					FieldInfo fieldValueinfo = type.GetField("fieldValue");
					if (TypeHelper.IsEnumType(originType))
					{
						fieldValue = fieldValue.ToString();
					}
					fieldValueinfo.SetValue(comp, fieldValue);
				}
				catch (Exception e)
				{
					throw new Exception($"transform failed,nodeName:{nodeData.Name}  fieldName:{args.Key} fieldType:{originType}", e);
				}
			}
			foreach (string key in unUseList)
			{
				nodeData.Args.Remove(key);
			}
			foreach (BehaviorNodeData child in nodeData.children)
			{
				BehaviorNodeConfig childConfig = this.BehaviorNodeDataToNodeConfig(child);
				childConfig.gameObject.transform.parent = nodeConfig.gameObject.transform;
			}
			return nodeConfig;
		}

		public BehaviorNodeData NodeProtoToBehaviorNodeData(NodeProto nodeProto)
		{
			BehaviorNodeData nodeData = new BehaviorNodeData
			{
				Id = nodeProto.Id,
				Name = nodeProto.Name,
				Desc = nodeProto.Desc,
				Args = nodeProto.Args,
				children = new List<BehaviorNodeData>()
			};
			foreach (NodeProto child in nodeProto.children)
			{
				nodeData.children.Add(this.NodeProtoToBehaviorNodeData(child));
			}
			return nodeData;
		}

		public NodeProto BehaviorNodeDataToNodeProto(BehaviorNodeData nodeData)
		{
			NodeProto nodeProto = new NodeProto
			{
				Id = nodeData.Id,
				Name = nodeData.Name,
				Desc = nodeData.Desc,
				Args = nodeData.Args,
				children = new List<NodeProto>()
			};
			foreach (BehaviorNodeData child in nodeData.children)
			{
				nodeProto.children.Add(this.BehaviorNodeDataToNodeProto(child));
			}
			return nodeProto;
		}

		//private List<NodeFieldDesc> GetFieldDescList(NodeProto nodeProto, Type type)
		//{
		//	List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.name, type);
		//	foreach (NodeProto childProto in nodeProto.children)
		//	{
		//		list.AddRange(GetFieldDescList(childProto, type));
		//	}
		//	return list;
		//}

		public BehaviorNodeData CreateNode(int treeId, string nodeName)
		{
			if (!this.name2NodeProtoDict.ContainsKey(nodeName))
			{
				Debug.LogError($"节点类型:{nodeName}不存在");
				return null;
			}
			BehaviorNodeData node = new BehaviorNodeData(nodeName);
			node.Id = AutoNodeId();
			node.Name = nodeName;
			return node;
		}

		public BehaviorNodeData CopyNode(BehaviorNodeData node)
		{
			BehaviorNodeData copyNode = new BehaviorNodeData();
			copyNode.Name = node.Name;
			copyNode.Desc = node.Desc;
			copyNode.Pos = node.Pos;
			copyNode.Args = node.Args.Clone();
			List<BehaviorNodeData> list = new List<BehaviorNodeData>();
			foreach (BehaviorNodeData item in node.children)
			{
				list.Add(item);
			}
			foreach (BehaviorNodeData child in list)
			{
				copyNode.AddChild(CopyNode(child));
			}
			copyNode.ResetId();
			return copyNode;
		}

		public void OpenBehaviorEditor(GameObject go)
		{
			if (go == null)
			{
				return;
			}
			selectNodeName = "";
			CurTreeGO = go;
			NewLoadData();
			BehaviorDesignerWindow.ShowWindow();
			Game.Scene.GetComponent<EventComponent>().Run(EventIdType.BehaviorTreeOpenEditor);
		}

		public string[] GetCanInPutEnvKeyArray(BehaviorNodeData nodeData, NodeFieldDesc desc)
		{
			List<string> list1 = new List<string>();
			list1.AddRange(Instance.GetNodeOutPutEnvKeyList(nodeData, desc));
			list1.Add(BTEnvKey.None);
			HashSet<string> hashSet = new HashSet<string>();
			foreach (string item in list1)
			{
				hashSet.Add(item);
			}

			string[] strArr = new string[hashSet.Count];
			int i = 0;
			foreach (string str in hashSet)
			{
				strArr[i++] = str;
			}
			return strArr;
		}

		public List<string> GetCanInPutEnvKeyList(BehaviorNodeData nodeData, NodeFieldDesc desc)
		{
			List<string> list1 = Instance.GetNodeOutPutEnvKeyList(nodeData, desc);
			HashSet<string> hashSet = new HashSet<string>();
			foreach (string item in list1)
			{
				hashSet.Add(item);
			}
			List<string> resultList = new List<string>();
			foreach (string item in hashSet)
			{
				resultList.Add(item);
			}
			return resultList;
		}

		public List<string> GetNodeOutPutEnvKeyList(BehaviorNodeData nodeData, NodeFieldDesc desc = null)
		{
			NodeProto rootNode = this.BehaviorNodeDataToNodeProto(CurTree.Root);
			NodeProto inputNode = this.BehaviorNodeDataToNodeProto(nodeData);
			List<NodeFieldDesc> descList = _GetNodeOutPutEnvKeyList(rootNode, inputNode, desc);
			List<string> list = new List<string>();
			foreach (NodeFieldDesc item in descList)
			{
				string str = item.value?.ToString() ?? "";
				list.Add(str);
			}
			return list;
		}

		private static List<NodeFieldDesc> _GetNodeOutPutEnvKeyList(NodeProto nodeProto, NodeProto inputNode, NodeFieldDesc desc = null)
		{
			if (nodeProto.Id >= inputNode.Id)
			{
				return new List<NodeFieldDesc>();
			}
			List<NodeFieldDesc> list = new List<NodeFieldDesc>();

			if (desc == null)
			{
				list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.Name, typeof(NodeOutputAttribute));
			}
			else
			{
				list = ExportNodeTypeConfig.GetNodeFieldInOutPutFilterDescList(nodeProto.Name, typeof(NodeOutputAttribute), desc.envKeyType);
			}
			for (int i = 0; i < list.Count; i++)
			{
				object value = nodeProto.Args.Get(list[i].name);
				list[i].value = value;
			}

			foreach (NodeProto childProto in nodeProto.children)
			{
				list.AddRange(_GetNodeOutPutEnvKeyList(childProto, inputNode, desc));
			}
			return list;
		}

		public List<string> GetSelectNodeInputValueList(NodeProto nodeProto)
		{
			List<string> resultList = new List<string>();
			List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.Name, typeof(NodeInputAttribute));

			foreach (NodeFieldDesc desc in list)
			{
				string value = nodeProto.Args.Get(desc.name)?.ToString();
				resultList.Add(value);
			}
			return resultList;
		}
		
		private List<string> inputValueList;

		public void SelectNode(BehaviorNodeData node)
		{
			NodeProto nodeProto = this.BehaviorNodeDataToNodeProto(node);
			inputValueList = GetSelectNodeInputValueList(nodeProto);
		}

		public bool IsHighLight(BehaviorNodeData node)
		{
			NodeProto nodeProto = this.BehaviorNodeDataToNodeProto(node);
			List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.Name, typeof(NodeOutputAttribute));
			foreach (NodeFieldDesc desc in list)
			{
				if (!nodeProto.Args.ContainsKey(desc.name))
				{
					continue;
				}
				string value = nodeProto.Args.Get(desc.name)?.ToString();
				List<string> resultList = inputValueList.FindAll(str => { return str == value; });
				if (resultList.Count > 0)
				{
					return true;
				}
			}
			return false;
		}

		public void SetDebugState(BehaviorTree tree, List<long> pathList)
		{
			CurBehaviorTree = tree;
			foreach (long nodeId in pathList)
			{
				Instance.SetDebugState(tree, (int) nodeId);
			}
		}

		private void SetDebugState(BehaviorTree tree, int nodeId)
		{
			if (this.BehaviorTreeConfig != null && tree.behaviorTreeConfig.gameObject.name == this.BehaviorTreeConfig.gameObject.name)
			{
				BehaviorNodeData nodeData = GetNodeData(CurTree.Root, nodeId);
				if (nodeData != null)
				{
					nodeData.NodeDeubgState = DebugState.True;
				}
			}
		}

		public void ClearDebugState()
		{
			_ClearDebugState(CurTree.Root);
		}

		private static void _ClearDebugState(BehaviorNodeData nodeData)
		{
			nodeData.NodeDeubgState = DebugState.Normal;
			foreach (BehaviorNodeData child in nodeData.children)
			{
				_ClearDebugState(child);
			}
		}

		public BehaviorNodeData GetNodeData(BehaviorNodeData nodeData, int nodeId)
		{
			BehaviorNodeData result = null;
			if (nodeData.Id == nodeId)
			{
				return nodeData;
			}
			foreach (BehaviorNodeData child in nodeData.children)
			{
				result = GetNodeData(child, nodeId);
				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		public BehaviorNodeData GetNode(BehaviorNodeData nodeData, int nodeId)
		{
			if (nodeData.Id == nodeId)
			{
				return nodeData;
			}
			foreach (BehaviorNodeData data in nodeData.children)
			{
				return GetNode(data, nodeId);
			}
			return null;
		}

		public void Clear()
		{
			treePathList.Clear();
			CurBehaviorTree = null;
		}
	}
}