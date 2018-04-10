using System;
using System.Collections.Generic;
using ETModel;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETEditor
{
	public class PropertyDesigner: Editor
	{
		private readonly string[] mBehaviorToolbarStrings = { "属性值", "节点", "工具", "调试" };
		private int mBehaviorToolbarSelection;
		private float mWidth = 380f;
		private BehaviorNodeData mCurBehaviorNode;
		private string mSearchNode = "";
		private FoldoutNode mCurNode;

		public PropertyDesigner()
		{
			Init();
		}

		private void Init()
		{
			UpdateList();
		}

		private bool mDragingBorder;

		public void HandleEvents()
		{
			Event e = Event.current;
			switch (e.type)
			{
				case EventType.MouseDown:
					if (e.button == 0 && e.mousePosition.x < mWidth + 30 && e.mousePosition.x > mWidth)
					{
						mDragingBorder = true;
					}
					break;
				case EventType.MouseDrag:
					if (mDragingBorder)
					{
						mWidth += e.delta.x;
					}
					break;
				case EventType.MouseUp:
					mDragingBorder = false;
					break;
			}
		}

		private Rect toolbarRect = new Rect(0f, 0f, 0, 0);

		public void Draw()
		{
			HandleEvents();
			toolbarRect = new Rect(0f, 0f, mWidth, 18f);
			Rect boxRect = new Rect(0f, toolbarRect.height, this.mWidth, (Screen.height - toolbarRect.height) - 21f);
			GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
			this.mBehaviorToolbarSelection = GUILayout.Toolbar(this.mBehaviorToolbarSelection, this.mBehaviorToolbarStrings, EditorStyles.toolbarButton);
			GUILayout.EndArea();
			GUILayout.BeginArea(boxRect);
			Filter();
			if (mBehaviorToolbarSelection == 0)
			{
				DrawValueView();
			}
			else if (mBehaviorToolbarSelection == 1)
			{
				DrawNodes();
			}
			else if (mBehaviorToolbarSelection == 2)
			{
			}
			else if (mBehaviorToolbarSelection == 3)
			{
				DrawDebugView();
			}
			GUILayout.EndArea();
		}

		public void SetToolBar(int select)
		{
			this.mBehaviorToolbarSelection = select;
		}

		public void SelectNodeFolderCallback(FoldoutFolder folder)
		{
			folder.Select = true;
			if (mCurNodeFolder != null && mCurNodeFolder != folder)
			{
				mCurNodeFolder.Select = false;
				mCurNodeFolder = null;
			}
			mCurNodeFolder = folder;
		}

		public void SelectNodeCallback(FoldoutNode node)
		{
			node.Select = true;
			if (mCurNode != null && mCurNode != node)
			{
				mCurNode.Select = false;
				mCurNode = null;
			}
			mCurNode = node;
		}

		private void UpdateList()
		{
			mNodeFoldout = new FoldoutFolder("所有节点", SelectNodeFolderCallback);
			mNodeFoldout.Fold = true;

			BTNodeInfoComponent btNodeInfoComponent = BTEditor.Instance.GetComponent<BTNodeInfoComponent>();
			foreach (string classify in btNodeInfoComponent.GetAllClassify())
			{ 
				List<NodeMeta> nodeTypeList = btNodeInfoComponent.GetNodeMetas(classify);
				FoldoutFolder folder = mNodeFoldout.AddFolder(classify, SelectNodeFolderCallback);
				folder.Fold = true;

				mNodeCount++;
				foreach (NodeMeta nodeType in nodeTypeList)
				{
					folder.AddNode(classify, nodeType.name + " (" + nodeType.describe + ")", SelectNodeCallback);
					mNodeCount++;
				}
			}
		}

		private Vector2 mTreeScrollPos = Vector2.zero;
		private int mNodeCount;
		private FoldoutFolder mNodeFoldout;
		private FoldoutFolder mCurNodeFolder;

		private void DrawNodes()
		{
			float offset = 190f;
			if (mCurNode == null)
			{
				offset = 55f;
			}
			DrawSearchList(offset);
			DrawNodeFunctions(offset);
		}
		
		private int mEnumNodeTypeSelection;
		string[] mEnumNodeTypeArr;

		private string DrawSearchList(float offset)
		{
			GUILayout.BeginHorizontal();
			GUI.SetNextControlName("Search");
			this.mSearchNode = GUILayout.TextField(this.mSearchNode, GUI.skin.FindStyle("ToolbarSeachTextField"));
			GUILayout.EndHorizontal();

			toolbarRect = new Rect(0f, 15f, mWidth, 25f);
			GUILayout.BeginArea(toolbarRect, EditorStyles.toolbar);
			GUILayout.BeginHorizontal();

			GUILayout.Label("Filter");
			Array strArr = Enum.GetValues(typeof(NodeClassifyType));
			List<string> strList = new List<string>();
			strList.Add("All");
			foreach (object str in strArr)
			{
				strList.Add(str.ToString());
			}
			mEnumNodeTypeArr = strList.ToArray();
			mEnumNodeTypeSelection = EditorGUILayout.Popup(mEnumNodeTypeSelection, mEnumNodeTypeArr);
			if (GUILayout.Button("Clear"))
			{
				ClearNodes();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(0f, 15f + 20, this.mWidth, Screen.height - offset));
			this.mTreeScrollPos = GUI.BeginScrollView(new Rect(0f, 0f, this.mWidth, Screen.height - offset), this.mTreeScrollPos,
			                                          new Rect(0f, 0f, this.mWidth - 20f, mNodeCount * 19), false, false);
			mNodeFoldout.Draw();
			GUI.EndScrollView();
			GUILayout.EndArea();
			if (mCurNode != null)
			{
				string[] arr = mCurNode.Text.Split(' ');
				string name = arr[0];
				return name;
			}
			return "";
		}

		private void DrawNodeFunctions(float offset)
		{
			Rect boxRect = new Rect(0f, Screen.height - offset + 15f, this.mWidth, 200f);
			GUILayout.BeginArea(boxRect);
			BTEditor.Instance.selectNodeName = "";
			if (mCurNode != null)
			{
				string[] arr = mCurNode.Text.Split(' ');
				string name = arr[0];
				BTEditor.Instance.selectNodeName = name;
				BTEditor.Instance.selectNodeType = mCurNode.folderName;
				if (mCurNode.folderName != NodeClassifyType.Root.ToString())
				{
					if (GUILayout.Button("新建"))
					{
						Game.EventSystem.Run(EventIdType.BehaviorTreePropertyDesignerNewCreateClick, name, Vector2.zero);
					}
				}
				if (mCurNode.folderName != NodeClassifyType.Root.ToString() ||
				    (mCurNode.folderName == NodeClassifyType.Root.ToString() && mCurBehaviorNode.IsRoot()))
				{
					if (GUILayout.Button("替换"))
					{
						Game.EventSystem.Run(EventIdType.BehaviorTreeReplaceClick, name, Vector2.zero);
					}
				}

				if (GUILayout.Button("保存"))
				{
					BTEditor.Instance.SaveAll();
				}
				NodeMeta node = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(name);
				GUILayout.Label("节点名:" + node.name);
				GUILayout.Label("描述:" + node.describe);
			}

			GUILayout.EndArea();
		}

		private void ClearNodes()
		{
			BTEditor.Instance.selectNodeName = "";
			mEnumNodeTypeSelection = 0;
			mSearchNode = "";
			foreach (FoldoutFolder folder in mNodeFoldout.Folders)
			{
				foreach (FoldoutNode node in folder.Nodes)
				{
					node.Hide = false;
				}
			}
		}

		private void Filter()
		{
			foreach (FoldoutFolder folder in mNodeFoldout.Folders)
			{
				string selectType;
				if (mEnumNodeTypeSelection == 0)
				{
					selectType = "All";
				}
				else
				{
					selectType = Enum.GetName(typeof(NodeClassifyType), mEnumNodeTypeSelection - 1);
				}

				if (selectType == folder.Text || selectType == "All")
				{
					folder.Hide = false;
					foreach (FoldoutNode node in folder.Nodes)
					{
						if (node.Text.ToUpper().IndexOf(mSearchNode.ToUpper(), StringComparison.Ordinal) == -1)
						{
							node.Hide = true;
						}
						else
						{
							node.Hide = false;
						}
					}
				}
				else
				{
					foreach (FoldoutNode node in folder.Nodes)
					{
						node.Hide = true;
					}
					folder.Hide = true;
				}
			}
		}
		
		private readonly GameObject[] searchGoArr = new GameObject[0];
		private Vector2 scrollPosition = Vector2.zero;

		private void ShowResult()
		{
			Rect boxRect1 = new Rect(0f, 100, this.mWidth, Screen.height);
			GUILayout.BeginArea(boxRect1);

			scrollPosition = GUI.BeginScrollView(new Rect(0, 0, this.mWidth, 220), scrollPosition, new Rect(0, 0, this.mWidth - 20, searchGoArr.Length * 20));
			for (int i = 0; i < this.searchGoArr.Length; i++)
			{
				searchGoArr[i] = BehaviourTreeField((i + 1).ToString(), searchGoArr[i]);
			}

			GUI.EndScrollView();
			GUILayout.EndArea();
		}
		
		public GameObject BehaviourTreeField(string desc, GameObject value)
		{
			EditorGUILayout.BeginHorizontal();
			value = (GameObject) EditorGUILayout.ObjectField(desc, value, typeof(GameObject), false);
			if (value.GetComponent<BehaviorTreeConfig>() != null && GUILayout.Button("打开行为树"))
			{
				BTEditor.Instance.OpenBehaviorEditor(value);
				SetToolBar(2);
			}
			EditorGUILayout.EndHorizontal();
			return value;
		}

		private void DrawValueView()
		{
			if (mCurBehaviorNode?.Proto == null)
			{
				return;
			}
			if (GUILayout.Button("保存行为树"))
			{
				BTEditor.Instance.SaveAll();
			}
			NodeMeta proto = mCurBehaviorNode.Proto;
			GUILayout.Space(10f);
			GUILayout.Label("节点ID:" + mCurBehaviorNode.Id);
			GUILayout.Label("节点名:" + mCurBehaviorNode.Name);
			GUILayout.Label("说明:");
			GUILayout.Label(proto.describe);
			GUILayout.Label("描述:");
			mCurBehaviorNode.Desc = EditorGUILayout.TextArea(mCurBehaviorNode.Desc, GUILayout.Height(50f));

			DrawAllValue(proto);
		}

		private bool mFoldParam = true;
		private bool mFoldInput = true;
		private bool mFoldOutput = true;

		private void DrawAllValue(NodeMeta proto)
		{
			List<NodeFieldDesc> paramFieldList = GetFieldDescList(proto.new_args_desc, typeof(NodeFieldAttribute));
			List<NodeFieldDesc> inputFieldList = GetFieldDescList(proto.new_args_desc, typeof(NodeInputAttribute));
			List<NodeFieldDesc> outputFieldList = GetFieldDescList(proto.new_args_desc, typeof(NodeOutputAttribute));
			mFoldParam = EditorGUILayout.Foldout(mFoldParam, "参数");
			if (mFoldParam)
			{
				DrawProp(proto.name, paramFieldList, NodeParamType.None);
			}
			mFoldInput = EditorGUILayout.Foldout(mFoldInput, "输入");
			if (mFoldInput)
			{
				DrawProp(proto.name, inputFieldList, NodeParamType.Input);
			}
			mFoldOutput = EditorGUILayout.Foldout(mFoldOutput, "输出");
			if (mFoldOutput)
			{
				DrawProp(proto.name, outputFieldList, NodeParamType.Output);
			}
		}

		private List<NodeFieldDesc> GetFieldDescList(List<NodeFieldDesc> fieldList, Type fieldAttributeType)
		{
			List<NodeFieldDesc> newFieldList = new List<NodeFieldDesc>();
			for (int i = 0; i < fieldList.Count; i++)
			{
				if (fieldList[i].attributeType == fieldAttributeType)
				{
					newFieldList.Add(fieldList[i]);
				}
			}
			return newFieldList;
		}

		private void DrawProp(string nodeName, List<NodeFieldDesc> fieldList, NodeParamType nodeParamType)
		{
			for (int i = 0; i < fieldList.Count; i++)
			{
				NodeFieldDesc desc = fieldList[i];
				Type fieldType = NodeMetaHelper.GetFieldType(nodeName, desc.name);
				NodeMeta nodeMeta = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(nodeName);

				// 如果不存在这个参数，给一个默认的
				if (!mCurBehaviorNode.Args.ContainsKey(desc.name))
				{
					object obj = desc.value ?? BTTypeManager.GetDefaultValue(fieldType);
					mCurBehaviorNode.Args.Add(desc.name, obj);
				}

				object newValue = null;
				if (TypeHelper.IsStringType(fieldType))
				{
					if (nodeParamType == NodeParamType.Input)
					{
						newValue = InputEnumFieldValue(desc);
					}
					else if (nodeParamType == NodeParamType.Output && nodeMeta.classify == NodeClassifyType.Root.ToString())
					{
						newValue = ConstTextFieldValue(desc);
					}
					else
					{
						newValue = TextFieldValue(desc);
					}
				}
				else if (TypeHelper.IsFloatType(fieldType))
				{
					newValue = FloatFieldValue(desc);
				}
				else if (TypeHelper.IsDoubleType(fieldType))
				{
					newValue = DoubletFieldValue(desc);
				}
				else if (TypeHelper.IsIntType(fieldType))
				{
					newValue = IntFieldValue(desc);
				}
				else if (TypeHelper.IsLongType(fieldType))
				{
					newValue = LongFieldValue(desc);
				}
				else if (TypeHelper.IsBoolType(fieldType))
				{
					newValue = BoolFieldValue(desc);
				}
				else if (TypeHelper.IsObjectType(fieldType))
				{
					newValue = ObjectFieldValue(desc);
				}
				else if (TypeHelper.IsIntArrType(fieldType))
				{
					newValue = IntArrFieldValue(desc);
				}
				else if (TypeHelper.IsLongArrType(fieldType))
				{
					newValue = LongArrFieldValue(desc);
				}
				else if (TypeHelper.IsStringArrType(fieldType))
				{
					newValue = StrArrFieldValue(desc);
				}
				else if (TypeHelper.IsFloatArrType(fieldType))
				{
					newValue = FloatArrFieldValue(desc);
				}
				else if (TypeHelper.IsDoubleArrType(fieldType))
				{
					newValue = DoubleArrFieldValue(desc);
				}
				else if (TypeHelper.IsEnumType(fieldType))
				{
					newValue = EnumFieldValue(desc);
				}
				else if (TypeHelper.IsObjectArrayType(fieldType))
				{
					newValue = ObjectArrFieldValue(desc);
				}
				else
				{
					Log.Error($"行为树节点暂时未支持此类型:{fieldType}！");
					return;
				}
				mCurBehaviorNode.Args.SetKeyValueComp(desc.name, newValue);
			}
		}

		private object ObjectFieldValue(NodeFieldDesc desc)
		{
			Object oldValue = (Object) mCurBehaviorNode.Args.Get(desc.name);
			EditorGUILayout.LabelField(GetPropDesc(desc));
			Object newValue = EditorGUILayout.ObjectField("", oldValue, desc.type, false);
			if (newValue == null)
			{
				return null;
			}
			if (TypeHelper.IsGameObjectType(desc.type) && !BehaviorTreeArgsDict.SatisfyCondition((GameObject) newValue, desc.constraintTypes))
			{
				return null;
			}
			return newValue;
		}

		private object ConstTextFieldValue(NodeFieldDesc desc)
		{
			string oldValue = desc.value.ToString();
			EditorGUILayout.LabelField(GetPropDesc(desc));
			EditorGUILayout.LabelField("", oldValue);
			return oldValue;
		}

		private object TextFieldValue(NodeFieldDesc desc)
		{
			string oldValue = (string)mCurBehaviorNode.Args.Get(desc.name);
			EditorGUILayout.LabelField(GetPropDesc(desc));
			object newValue = EditorGUILayout.TextField("", oldValue);
			return newValue;
		}

		private object BoolFieldValue(NodeFieldDesc desc)
		{
			bool oldValue = (bool)mCurBehaviorNode.Args.Get(desc.name);
			EditorGUILayout.LabelField(GetPropDesc(desc));
			object newValue = EditorGUILayout.Toggle("", oldValue);
			return newValue;
		}

		private object IntFieldValue(NodeFieldDesc desc)
		{
			int oldValue = (int)mCurBehaviorNode.Args.Get(desc.name);
			EditorGUILayout.LabelField(GetPropDesc(desc));
			object newValue = EditorGUILayout.IntField("", oldValue);
			return newValue;
		}

		private object LongFieldValue(NodeFieldDesc desc)
		{
			long oldValue = (long)mCurBehaviorNode.Args.Get(desc.name);
			EditorGUILayout.LabelField(GetPropDesc(desc));
			object newValue = EditorGUILayout.LongField("", oldValue);
			return newValue;
		}

		private object FloatFieldValue(NodeFieldDesc desc)
		{
			float oldValue = (float)mCurBehaviorNode.Args.Get(desc.name);
			EditorGUILayout.LabelField(GetPropDesc(desc));
			object newValue = EditorGUILayout.FloatField("", oldValue);
			return newValue;
		}

		private object DoubletFieldValue(NodeFieldDesc desc)
		{
			double oldValue = (double)mCurBehaviorNode.Args.Get(desc.name);
			EditorGUILayout.LabelField(GetPropDesc(desc));
			object newValue = EditorGUILayout.DoubleField("", oldValue);
			return newValue;
		}

		private bool foldStrArr;

		private object StrArrFieldValue(NodeFieldDesc desc)
		{
			string[] oldValue = (string[])mCurBehaviorNode.Args.Get(desc.name);
			string[] newValue = CustomArrayField.StringArrFieldValue(ref foldStrArr, GetPropDesc(desc), oldValue);
			return newValue;
		}

		private bool foldIntArr;

		private object IntArrFieldValue(NodeFieldDesc desc)
		{
			int[] oldValue = (int[])mCurBehaviorNode.Args.Get(desc.name);
			int[] newValue = CustomArrayField.IntArrFieldValue(ref foldIntArr, GetPropDesc(desc), oldValue);
			return newValue;
		}

		private bool foldLongArr;

		private object LongArrFieldValue(NodeFieldDesc desc)
		{
			long[] oldValue = (long[])mCurBehaviorNode.Args.Get(desc.name);
			long[] newValue = CustomArrayField.LongArrFieldValue(ref foldLongArr, GetPropDesc(desc), oldValue);
			return newValue;
		}

		private bool foldFloatArr;

		private object FloatArrFieldValue(NodeFieldDesc desc)
		{
			float[] oldValue = (float[])mCurBehaviorNode.Args.Get(desc.name);
			float[] newValue = CustomArrayField.FloatArrFieldValue(ref foldFloatArr, GetPropDesc(desc), oldValue);
			return newValue;
		}

		private bool foldDoubleArr;

		private object DoubleArrFieldValue(NodeFieldDesc desc)
		{
			double[] oldValue = (double[])mCurBehaviorNode.Args.Get(desc.name);
			double[] newValue = CustomArrayField.DoubleArrFieldValue(ref foldDoubleArr, GetPropDesc(desc), oldValue);
			return newValue;
		}

		private bool foldObjectArr;

		private object ObjectArrFieldValue(NodeFieldDesc desc)
		{
			Object[] oldValue = (Object[])mCurBehaviorNode.Args.Get(desc.name);
			Object[] newValue = CustomArrayField.ObjectArrFieldValue(ref foldObjectArr, GetPropDesc(desc), oldValue, desc);
			return newValue;
		}

		private object OutPutEnumFieldValue(NodeFieldDesc desc)
		{
			string oldValue = mCurBehaviorNode.Args.Get(desc.name)?.ToString();
			if (string.IsNullOrEmpty(oldValue))
			{
				oldValue = BTEnvKey.None;
			}
			string[] enumValueArr;
			if (mCurBehaviorNode.IsRoot() && desc.value.ToString() != BTEnvKey.None)
			{
				enumValueArr = new string[] { desc.value.ToString() };
			}
			else
			{
				enumValueArr = BehaviorTreeInOutConstrain.GetEnvKeyEnum(typeof(BTEnvKey));
				if (enumValueArr.Length == 0)
				{
					enumValueArr = new string[] { BTEnvKey.None };
				}
				if (oldValue == BTEnvKey.None)
				{
					oldValue = desc.value.ToString();
				}
			}

			int oldSelect = IndexInStringArr(enumValueArr, oldValue);
			string label = desc.name + (desc.desc == ""? "" : $"({desc.desc})") + $"({desc.envKeyType})";
			EditorGUILayout.LabelField(label);
			int selection = EditorGUILayout.Popup("", oldSelect, enumValueArr);
			string newValue = enumValueArr[selection];
			return newValue;
		}

		private object InputEnumFieldValue(NodeFieldDesc desc)
		{
			string oldValue = mCurBehaviorNode.Args.Get(desc.name)?.ToString();
			string[] enumValueArr = BTEditor.Instance.GetCanInPutEnvKeyArray(mCurBehaviorNode, desc);
			if (enumValueArr.Length == 0)
			{
				enumValueArr = new string[1] { BTEnvKey.None };
			}
			else if (string.IsNullOrEmpty(oldValue))
			{
				oldValue = enumValueArr[0];
			}
			int oldSelect = IndexInStringArr(enumValueArr, oldValue);
			string label = desc.name + (desc.desc == ""? "" : $"({desc.desc})") + $"({desc.envKeyType})";
			EditorGUILayout.LabelField(label);
			int selection = EditorGUILayout.Popup("", oldSelect, enumValueArr);
			string newValue = enumValueArr[selection];
			return newValue;
		}

		private int IndexInStringArr(string[] strArr, string str)
		{
			for (int i = 0; i < strArr.Length; i++)
			{
				if (strArr[i] == str)
				{
					return i;
				}
			}
			return 0;
		}

		private object EnumFieldValue(NodeFieldDesc desc)
		{
			string oldValue = mCurBehaviorNode.Args.Get(desc.name)?.ToString();
			if (string.IsNullOrEmpty(oldValue))
			{
				oldValue = GetDefaultEnumValue(desc.type);
			}
			Enum oldValueEnum = (Enum) Enum.Parse(desc.type, oldValue);
			EditorGUILayout.LabelField(desc.type.ToString());
			Enum newValueEnum = EditorGUILayout.EnumPopup(oldValueEnum);
			return newValueEnum.ToString();
		}

		private string GetDefaultEnumValue(Type type)
		{
			Array array = Enum.GetValues(type);
			string value = array.GetValue(0).ToString();
			return value;
		}

		public string[] GetEnumValues(Type enumType)
		{
			List<string> enumValueList = new List<string>();
			foreach (int myCode in Enum.GetValues(enumType))
			{
				string strName = Enum.GetName(enumType, myCode);
				enumValueList.Add(strName);
			}
			return enumValueList.ToArray();
		}

		public string GetPropDesc(NodeFieldDesc desc)
		{
			string typeDesc = desc.type.ToString().Split('.')[1].ToLower();
			return desc.name + desc.desc + "(" + typeDesc + ")";
		}

		public void onSelectNode(params object[] list)
		{
			mCurBehaviorNode = (BehaviorNodeData) list[0];
			GUI.FocusControl("");
		}

		public void DrawDebugView()
		{
			BTDebugComponent btDebugComponent = BTEditor.Instance.GetComponent<BTDebugComponent>();
			List<List<long>> treePathList = btDebugComponent.Get(btDebugComponent.OwnerId);
			GUILayout.BeginHorizontal();
			GUILayout.Label("行为树Id:");
			btDebugComponent.OwnerId = EditorGUILayout.LongField(btDebugComponent.OwnerId);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("清空执行记录"))
			{
				treePathList.Clear();
				BTEditor.Instance.ClearDebugState();
			}

			if (GUILayout.Button("清除帧选择"))
			{
				btDebugComponent.IsFrameSelected = false;
			}
			GUILayout.EndHorizontal();

			const float offset = 55f;
			GUILayout.BeginArea(new Rect(0f, 60f, this.mWidth, Screen.height - offset));
			this.mTreeScrollPos = GUI.BeginScrollView(
				new Rect(0f, 0f, this.mWidth, Screen.height - offset), this.mTreeScrollPos,
			    new Rect(0f, 0f, this.mWidth - 20f, treePathList.Count * 22), false, false);

			
			for (int i = 0; i < treePathList.Count; i++)
			{
				if (GUILayout.Button($"frame{i}"))
				{
					btDebugComponent.IsFrameSelected = true;
					BTEditor.Instance.ClearDebugState();
					BTEditor.Instance.SetDebugState(treePathList[i]);
				}
			}
			GUI.EndScrollView();
			GUILayout.EndArea();
		}
	}
}