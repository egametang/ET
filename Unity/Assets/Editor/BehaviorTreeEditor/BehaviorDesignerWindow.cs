using Model;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
    public enum SubWinType
    {
        CreateNode,
        ReplaceNode,
    }
	public class MessageBoxArgs: EventArgs
	{
		public string msg;
	}

	public class BehaviorDesignerWindow: EditorWindow
	{
		private GraphDesigner mGraphDesigner;
		private PropertyDesigner mPropDesigner;
        private RightDesigner mRightDesigner;
        private static bool mShowSubWin;
        private SubWinType mSubWinType;
        private BehaviorTreeNodeClassPopup popUpMenu;
        public GraphDesigner GraphDesigner
		{
			get
			{
				return this.mGraphDesigner;
			}
		}

		public static BehaviorDesignerWindow Instance
		{
			get
			{
				return GetWindow<BehaviorDesignerWindow>(false, "行为树编辑器");
			}
		}
        public static bool IsShowSubWin
        {
            get
            {
                return mShowSubWin;
            }
        }
		public static void ShowWindow()
		{
			BehaviorDesignerWindow target = GetWindow<BehaviorDesignerWindow>("行为树编辑器", false);
            target.minSize = new Vector2(600f, 500f);
        }

        public void ShowSubWin(Vector2 pos,SubWinType subWinType)
        {
            mShowSubWin = true;
            popUpMenu.Show(windowRect, subWinType);
            windowRect.position = pos;
        }
        public void CloseSubWin()
        {
            mShowSubWin = false;
        }
        public static Rect windowRect = new Rect(400, 250, 400, 550);//子窗口的大小和位置
        public void DrawSubWindow()
        {
            BeginWindows();//标记开始区域所有弹出式窗口
            windowRect = GUILayout.Window(1, windowRect, DoWindow, "行为树节点");//创建内联窗口,参数分别为id,大小位置，创建子窗口的组件的函数，标题
            EndWindows();//标记结束
        }
        void DoWindow(int unusedWindowID)
        {
            popUpMenu.DrawSearchList();
            GUI.DragWindow();//画出子窗口
        }
        public void Awake()
        {
			mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();
			mPropDesigner = ScriptableObject.CreateInstance<PropertyDesigner>();
           // mRightDesigner = new RightDesigner();
            popUpMenu = new BehaviorTreeNodeClassPopup();
            popUpMenu.GraphDesigner = mGraphDesigner;
			//mGraphDesigner.onSelectTree();
		}

		public void OnGUI()
		{
			HandleEvents();
			mPropDesigner?.Draw();
			mGraphDesigner?.Draw(this.position);
          //  mRightDesigner?.Draw();
            if (mShowSubWin)
            {
                DrawSubWindow();
            }
            this.Repaint();
		}

		public void HandleEvents()
		{
			var e = Event.current;
			switch (e.type)
			{
				case EventType.KeyUp:
					if (e.keyCode == KeyCode.Escape || (e.keyCode == KeyCode.S && e.control))
					{
						BehaviorManager.GetInstance().SaveAll();
					}
                    else if (e.keyCode == KeyCode.F4)
                    {
                        BehaviorManager.GetInstance().SaveAll();
                    }
                    break;
                case EventType.MouseDown:
                    
                    break;
            }
		}

		public void OnDestroy()
		{
			BehaviorManager.GetInstance().Clear();
		}

		public void onUpdatePropList(params object[] list)
		{
			mPropDesigner.SetToolBar(0);
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
			mGraphDesigner.onSelectNode(list);
			mPropDesigner.onSelectNode(list);
    //      mRightDesigner.onSelectNode(list);
		}

		public void onStartConnect(NodeDesigner nodeDesigner, State state)
		{
			mGraphDesigner.onStartConnect(nodeDesigner, state);
		}

		public void onMouseInNode(BehaviorNodeData nodeData, NodeDesigner nodeDesigner)
		{
			mGraphDesigner.onMouseInNode(nodeData, nodeDesigner);
		}

		public void onCreateNode(string name, Vector2 pos)
		{
			mGraphDesigner.onCreateNode(name, pos);
		}

		public void onChangeNodeType(string name, Vector2 pos)
		{
			mGraphDesigner.onChangeNodeType(name, pos);
		}

		public NodeDesigner onCreateTree()
		{
			return mGraphDesigner.onCreateTree();
		}
        public void onDraggingRightDesigner(float deltaX)
        {
     //       mRightDesigner.onDraggingBorder(deltaX);
        }    
	}
}