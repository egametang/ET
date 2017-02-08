using System;
using System.Collections.Generic;
using Model;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Base;
using Object = UnityEngine.Object;

namespace MyEditor
{
	public class BehaviorManager
	{
		public GameObject CurTreeGO { get; set; }
		public BehaviorTreeData CurTree { get; set; }
        public const int NodeIdStartIndex = 100000;
        private int AutoID = NodeIdStartIndex;
        private Dictionary<string, ClientNodeTypeProto> mName2NodeProtoDict = new Dictionary<string, ClientNodeTypeProto>(); //节点类型 name索引
		private Dictionary<string, List<ClientNodeTypeProto>> mClassify2NodeProtoList = new Dictionary<string, List<ClientNodeTypeProto>>(); //节点分类 分类名索引
		private readonly Dictionary<int, NodeDesignerProto> mId2DesignerDict = new Dictionary<int, NodeDesignerProto>();
		private static BehaviorManager mInstance = new BehaviorManager();
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

		public static BehaviorManager GetInstance()
		{
			if (mInstance == null)
			{
				mInstance = new BehaviorManager();
			}
			return mInstance;
		}
         
		public Dictionary<string, List<ClientNodeTypeProto>> Classify2NodeProtoList
		{
			get
			{
				return mClassify2NodeProtoList;
			}
		}
       
        public List<ClientNodeTypeProto> AllNodeProtoList
        {
            get
            {
                List<ClientNodeTypeProto> list = new List<ClientNodeTypeProto>();
                foreach (var item in BehaviorManager.GetInstance().Classify2NodeProtoList)
                {
                    foreach (var proto in item.Value)
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
			mClassify2NodeProtoList = new Dictionary<string, List<ClientNodeTypeProto>>();
			foreach (var nodeType in mName2NodeProtoDict.Values)
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
				if (!mClassify2NodeProtoList.ContainsKey(classify))
				{
					mClassify2NodeProtoList.Add(classify, new List<ClientNodeTypeProto>());
				}
				mClassify2NodeProtoList[classify].Add(nodeType);
			}
		}

		public int AutoNodeId()
		{
            return ++AutoID;
		}
         
        public void NewLoadData()
        {
			Game.EntityEventManager.Register("Controller", DllHelper.GetController());

			LoadNodeTypeProto();
            NewLoadPrefabTree();
            FilterClassify();
        }

        public void LoadNodeTypeProto()
		{
			mName2NodeProtoDict = ExportNodeTypeConfig.ExportToDict();
		}
         
        public void NewLoadPrefabTree()
        {
            BehaviorTreeConfig config = CurTreeGO.GetComponent<BehaviorTreeConfig>();
            CurTree = BehaviorTreeConfigToTreeData(config);
        }
        
      
        public BehaviorTreeData BehaviorTreeConfigToTreeData(BehaviorTreeConfig config)
        {
            BehaviorTreeData tree = new BehaviorTreeData();
            tree.Root = NodeConfigToNodeData(config.RootNodeConfig);
            return tree;
        }

        public void printTree(BehaviorNodeData nodeData)
		{
			Log.Info($"printTree  :  {nodeData.nodeId} {nodeData}");
			foreach (var data in nodeData.children)
			{
				printTree(data);
			}
		}
        public bool CheckSatisfyInput()
        {
            NodeProto rootNode = NodeDataToNodeProto(CurTree.BehaviorNodeData);
            return CheckNodeInput(rootNode);
        }
       
		public bool CheckNodeInput(NodeProto nodeProto)
		{
			List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.name, typeof (NodeInputAttribute));
			foreach (var desc in list)
			{
				List<string> canInputList = GetCanInPutEnvKeyList(NodeProtoToNodeData(nodeProto), desc);
				string value = nodeProto.args_dict.GetTreeDictValue(desc.type, desc.name)?.ToString();
				List<string> resultList = canInputList.FindAll(str => { return str == value; });
				if (resultList.Count == 0)
				{
					Log.Error($"{nodeProto.name}节点(id:{nodeProto.nodeId})的{value}输入值非法!");
					return false;
				}
			}
			foreach (var child in nodeProto.children)
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
            GameObject.DestroyImmediate(config.gameObject);
        }
        public void ResetTreeId()
		{
            AutoID = NodeIdStartIndex;
            CurTree.Root.ResetId();
        }
        public void TransformTree(GameObject go)
        {
 
        }
 
		public void RemoveUnusedArgs(NodeProto nodeProto)
		{
			ClientNodeTypeProto proto = ExportNodeTypeConfig.GetNodeTypeProtoFromDll(nodeProto.name);
			List<string> unUsedList = new List<string>();
			foreach (var item in nodeProto.args_dict)
			{
				if (!proto.new_args_desc.Exists(a => (a.name == item.Key)))
				{
					unUsedList.Add(item.Key);
				}
			}
			foreach (var item in unUsedList)
			{
				nodeProto.args_dict.Remove(item);
			}
			for (int i = 0; i < nodeProto.children.Count; i++)
			{
				RemoveUnusedArgs(nodeProto.children[i]);
			}
		}

 
        private bool CheckHasTreeDesc()
        {
            if (string.IsNullOrEmpty(CurTree.Root.describe))
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
            CurTreeGO =  PrefabUtility.ReplacePrefab(config.gameObject, CurTreeGO,ReplacePrefabOptions.ReplaceNameBased);
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
           GameObject curTreeGo = GameObject.Instantiate(CurTreeGO);
           BehaviorTreeConfig config = curTreeGo.GetComponent<BehaviorTreeConfig>();
            if (config == null)
            {
                config = curTreeGo.AddComponent<BehaviorTreeConfig>();
            }
           foreach (Transform child in config.gameObject.transform)
           {
                GameObject.DestroyImmediate(child.gameObject);
           }
           try
           {
                config.RootNodeConfig = NodeDataToNodeConfig(tree.Root);
            }
           catch
           {
                Debug.LogError($"tree name : {tree.BehaviorNodeData.name}");
           }
            
            config.RootNodeConfig.gameObject.transform.parent = config.gameObject.transform;
            return config;
        }

        public BehaviorNodeData NodeConfigToNodeData(BehaviorNodeConfig nodeProto)
        {
            BehaviorNodeData nodeData = new BehaviorNodeData();
            nodeData.nodeId = nodeProto.id;
            nodeData.name = ((Object) nodeProto).name;
            nodeData.describe = nodeProto.describe;
            nodeData.args_dict = nodeProto.GetArgsDict();
            nodeData.children = new List<BehaviorNodeData>();
//             foreach (var item in nodeData.args_dict)
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
        public BehaviorNodeConfig NodeDataToNodeConfig(BehaviorNodeData nodeData)
        {
            GameObject go = new GameObject();
            BehaviorNodeConfig nodeConfig = go.AddComponent<BehaviorNodeConfig>();
            nodeConfig.id = nodeData.nodeId;
            ((Object) nodeConfig).name = nodeData.name;
            go.name = nodeData.name;
            nodeConfig.describe = nodeData.describe;
            List<string> unUseList = new List<string>();
            foreach (var args in nodeData.args_dict)
            {
                if (!ExportNodeTypeConfig.NodeHasField(nodeData.name,args.Key))
                {
                    unUseList.Add(args.Key);
                    continue;
                }
                Type originType = ExportNodeTypeConfig.GetFieldType(nodeData.name, args.Key);
                try
                {
                    string fieldName = args.Key;
                    object fieldValue = args.Value.GetValueByType(originType);
                    Type type = BTTypeManager.GetBTType(originType);
                    UnityEngine.Component comp = go.AddComponent(type);
                    FieldInfo fieldNameInfo = type.GetField("fieldName");
                    fieldNameInfo.SetValue(comp, fieldName);
                    FieldInfo fieldValueinfo = type.GetField("fieldValue");
                    if (BehaviorTreeArgsDict.IsEnumType(originType))
                    {
                        fieldValue = fieldValue.ToString();
                    }
                    fieldValueinfo.SetValue(comp, fieldValue);
                }
                catch (Exception e)
                {
                     throw new Exception($"transform failed,nodeName:{nodeData.name}  fieldName:{args.Key} fieldType:{originType}", e);
                }
            }
            foreach (string key in unUseList)
            {
                nodeData.args_dict.Remove(key);
            }
            foreach (var child in nodeData.children)
            {
                BehaviorNodeConfig childConfig = NodeDataToNodeConfig(child);
                childConfig.gameObject.transform.parent = nodeConfig.gameObject.transform;
            }
            return nodeConfig;
        }
        
        public BehaviorNodeData NodeProtoToNodeData(NodeProto nodeProto)
		{
			BehaviorNodeData nodeData = new BehaviorNodeData();
			nodeData.nodeId = nodeProto.nodeId;
			nodeData.name = nodeProto.name;
			nodeData.describe = nodeProto.describe;
			nodeData.args_dict = nodeProto.args_dict;
			nodeData.children = new List<BehaviorNodeData>();
			foreach (var child in nodeProto.children)
			{
				nodeData.children.Add(this.NodeProtoToNodeData(child));
			}
			return nodeData;
		}

		public NodeProto NodeDataToNodeProto(BehaviorNodeData nodeData)
		{
			NodeProto nodeProto = new NodeProto();
			nodeProto.nodeId = nodeData.nodeId;
			nodeProto.name = nodeData.name;
			nodeProto.describe = nodeData.describe;

			nodeProto.nodeIdList = new List<int>();
			nodeProto.args_dict = nodeData.args_dict;
			nodeProto.children = new List<NodeProto>();
			foreach (var child in nodeData.children)
			{
				nodeProto.children.Add(NodeDataToNodeProto(child));
                nodeProto.nodeIdList.Add(child.nodeId);
			}
			return nodeProto;
		}

		private List<NodeFieldDesc> GetFieldDescList(NodeProto nodeProto, Type type)
		{
			List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.name, type);
			foreach (NodeProto childProto in nodeProto.children)
			{
				list.AddRange(GetFieldDescList(childProto, type));
			}
			return list;
		}

		public BehaviorNodeData CreateNode(int treeId, string nodeName)
		{
			if (!mName2NodeProtoDict.ContainsKey(nodeName))
			{
				Debug.LogError($"节点类型:{nodeName}不存在");
				return null;
			}
			BehaviorNodeData node = new BehaviorNodeData(nodeName);
			node.nodeId = AutoNodeId();
			node.name = nodeName;
			return node;
		}

		public BehaviorNodeData CopyNode(BehaviorNodeData node)
		{
			BehaviorNodeData copyNode = new BehaviorNodeData();
			copyNode.name = node.name;
			copyNode.describe = node.describe;
			copyNode.Pos = node.Pos;
			copyNode.args_dict = new BehaviorTreeArgsDict();
			foreach (var item in node.args_dict)
			{
                ValueBase valueBase = ValueBase.Clone(item.Value);
				copyNode.args_dict.Add(item.Key, valueBase);
			}
			List<BehaviorNodeData> list = new List<BehaviorNodeData>();
			foreach (var item in node.children)
			{
				list.Add(item);
			}
			foreach (var child in list)
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
			list1.AddRange(GetInstance().GetNodeOutPutEnvKeyList(nodeData, desc));
            list1.Add(BTEnvKey.None);
            HashSet<string> hashSet = new HashSet<string>();
			foreach (var item in list1)
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
			List<string> list1 = GetInstance().GetNodeOutPutEnvKeyList(nodeData, desc);
			HashSet<string> hashSet = new HashSet<string>();
			foreach (var item in list1)
			{
				hashSet.Add(item);
			}
			List<string> resultList = new List<string>();
			foreach (var item in hashSet)
			{
				resultList.Add(item);
			}
			return resultList;
		}

		public List<string> GetNodeOutPutEnvKeyList(BehaviorNodeData nodeData, NodeFieldDesc desc = null)
		{
            NodeProto rootNode = NodeDataToNodeProto(CurTree.Root);
			NodeProto inputNode = NodeDataToNodeProto(nodeData);
			List<NodeFieldDesc> descList = _GetNodeOutPutEnvKeyList(rootNode, inputNode, desc);
			List<string> list = new List<string>();
			foreach (var item in descList)
			{
                string str = item.value?.ToString() ?? "";
				list.Add(str);
			}
			return list;
		}

		private List<NodeFieldDesc> _GetNodeOutPutEnvKeyList(NodeProto nodeProto, NodeProto inputNode, NodeFieldDesc desc = null)
		{
			if (nodeProto.nodeId >= inputNode.nodeId)
			{
				return new List<NodeFieldDesc>();
			}
			List<NodeFieldDesc> list = new List<NodeFieldDesc>();

			if (desc == null)
			{
				list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.name, typeof (NodeOutputAttribute));
			}
			else
			{
				list = ExportNodeTypeConfig.GetNodeFieldInOutPutFilterDescList(nodeProto.name, typeof (NodeOutputAttribute), desc.envKeyType);
			}
			for (int i = 0; i < list.Count; i++)
			{
				object value = nodeProto.args_dict.GetTreeDictValue(list[i].type, list[i].name);
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
			List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.name, typeof (NodeInputAttribute));

			foreach (var desc in list)
			{
				if (!nodeProto.args_dict.ContainsKey(desc.name))
				{
					ValueBase valueBase = new ValueBase();
					nodeProto.args_dict.Add(desc.name, valueBase);
				}
				if (string.IsNullOrEmpty(nodeProto.args_dict[desc.name].enumValue))
				{
					nodeProto.args_dict[desc.name].enumValue = BTEnvKey.None.ToString();
				}
				string value = nodeProto.args_dict.GetTreeDictValue(desc.type, desc.name)?.ToString();
				resultList.Add(value);
			}
			return resultList;
		}

		private BehaviorNodeData selectedNode;
		private List<string> inputValueList;

		public void SelectNode(BehaviorNodeData node)
		{
			selectedNode = node;
			NodeProto nodeProto = NodeDataToNodeProto(node);
			inputValueList = GetSelectNodeInputValueList(nodeProto);
		}

		public bool IsHighLight(BehaviorNodeData node)
		{
			NodeProto nodeProto = NodeDataToNodeProto(node);
			List<NodeFieldDesc> list = ExportNodeTypeConfig.GetNodeFieldInOutPutDescList(nodeProto.name, typeof (NodeOutputAttribute));
			foreach (var desc in list)
			{
                if (!nodeProto.args_dict.ContainsKey(desc.name))
                {
                    continue;
                }
				string value = nodeProto.args_dict.GetTreeDictValue(desc.type, desc.name)?.ToString();
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
			foreach (var nodeId in pathList)
			{
				GetInstance().SetDebugState(tree, (int) nodeId);
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

		private void _ClearDebugState(BehaviorNodeData nodeData)
		{
			nodeData.NodeDeubgState = DebugState.Normal;
			foreach (var child in nodeData.children)
			{
				_ClearDebugState(child);
			}
		}

		public BehaviorNodeData GetNodeData(BehaviorNodeData nodeData, int nodeId)
		{
			BehaviorNodeData result = null;
			if (nodeData.nodeId == nodeId)
			{
				return nodeData;
			}
			foreach (var child in nodeData.children)
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
			if (nodeData.nodeId == nodeId)
			{
				return nodeData;
			}
			foreach (var data in nodeData.children)
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