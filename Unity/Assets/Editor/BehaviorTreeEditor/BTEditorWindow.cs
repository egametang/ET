using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	public enum SubWinType
	{
		CreateNode,
		ReplaceNode
	}
	
	public class BTEditorWindow: EditorWindow
	{
		private PropertyDesigner propDesigner;
		private BehaviorTreeNodeClassPopup popUpMenu;
		public GraphDesigner GraphDesigner { get; private set; }

		public static BTEditorWindow Instance
		{
			get
			{
				return GetWindow<BTEditorWindow>(false, "行为树编辑器");
			}
		}

		public static bool IsShowSubWin { get; private set; }

		public static void ShowWindow()
		{
			BTEditorWindow target = GetWindow<BTEditorWindow>("行为树编辑器", false);
			target.minSize = new Vector2(600f, 500f);
		}

		public void ShowSubWin(Vector2 pos, SubWinType subWinType)
		{
			IsShowSubWin = true;
			popUpMenu.Show(windowRect, subWinType);
			windowRect.position = pos;
		}

		public void CloseSubWin()
		{
			IsShowSubWin = false;
		}

		public static Rect windowRect = new Rect(400, 250, 400, 550); //子窗口的大小和位置

		public void DrawSubWindow()
		{
			BeginWindows(); //标记开始区域所有弹出式窗口
			windowRect = GUILayout.Window(1, windowRect, DoWindow, "行为树节点"); //创建内联窗口,参数分别为id,大小位置，创建子窗口的组件的函数，标题
			EndWindows(); //标记结束
		}

		private void DoWindow(int unusedWindowID)
		{
			popUpMenu.DrawSearchList();
			GUI.DragWindow(); //画出子窗口
		}

		public void Awake()
		{
			this.GraphDesigner = CreateInstance<GraphDesigner>();
			this.propDesigner = CreateInstance<PropertyDesigner>();
			popUpMenu = new BehaviorTreeNodeClassPopup
			{
				GraphDesigner = this.GraphDesigner
			};
		}

		public void OnGUI()
		{
			HandleEvents();
			this.propDesigner?.Draw();
			this.GraphDesigner?.Draw(this.position);
			if (IsShowSubWin)
			{
				DrawSubWindow();
			}
			this.Repaint();
		}

		public void HandleEvents()
		{
			Event e = Event.current;
			switch (e.type)
			{
				case EventType.KeyUp:
					if (e.keyCode == KeyCode.Escape || (e.keyCode == KeyCode.S && e.control))
					{
						BTEditor.Instance.SaveAll();
					}
					else if (e.keyCode == KeyCode.F4)
					{
						BTEditor.Instance.SaveAll();
					}
					break;
				case EventType.MouseDown:
					break;
			}
		}

		public void OnDestroy()
		{
			BTEditor.Reset();
		}

		public void onUpdatePropList(params object[] list)
		{
			this.propDesigner.SetToolBar(0);
		}

		public void onShowMessage(params object[] list)
		{
			string msg = list[0].ToString();
			this.ShowNotification(new GUIContent(msg));
		}

		public void OnSelectNode(params object[] list)
		{
			if (list.Length == 0)
			{
				Debug.LogError(" node list can not be null");
				return;
			}
			this.GraphDesigner.onSelectNode(list);
			this.propDesigner.onSelectNode(list);
			//      mRightDesigner.onSelectNode(list);
		}

		public void onStartConnect(NodeDesigner nodeDesigner, State state)
		{
			this.GraphDesigner.onStartConnect(nodeDesigner, state);
		}

		public void onMouseInNode(BehaviorNodeData nodeData, NodeDesigner nodeDesigner)
		{
			this.GraphDesigner.onMouseInNode(nodeData, nodeDesigner);
		}

		public void onCreateNode(string name, Vector2 pos)
		{
			this.GraphDesigner.onCreateNode(name, pos);
		}

		public void onChangeNodeType(string name, Vector2 pos)
		{
			this.GraphDesigner.onChangeNodeType(name, pos);
		}

		public NodeDesigner onCreateTree()
		{
			return this.GraphDesigner.onCreateTree();
		}

		public void onDraggingRightDesigner(float deltaX)
		{
			//       mRightDesigner.onDraggingBorder(deltaX);
		}

		private void OnSelectionChange()
		{
			GameObject[] selectedGameObjects = Selection.gameObjects;
			if (selectedGameObjects.Length == 0)
			{
				return;
			}
			string[] ss = selectedGameObjects[0].name.Split('@');
			if (ss.Length < 2)
			{
				return;
			}
			long id = long.Parse(ss[1]);
			BTEditor.Instance.GetComponent<BTDebugComponent>().OwnerId = id;
		}
	}
}