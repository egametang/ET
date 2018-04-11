using System;
using System.Collections.Generic;
using ETModel;
using UnityEditor;
using UnityEngine;

namespace ETEditor
{
	public enum State
	{
		Normal, //普通
		Shift, //正在交换位置
		Ctrl, //正在移动位置
		Drag, //正在拖拽节点
		ConnectLeft, //正在连线左
		ConnectRight //正在连线右
	}

	public class GraphDesigner: Editor
	{
		public State mState = State.Normal;

		/// 绘图相关
		private Rect mBorderRect; //边框

		private Rect mGraphRect = new Rect(0, 0, 2000, 2000); //绘图区域
		private float mLeftWidth = 380f;
		private float mRightWidth;
		private Vector2 mScrollPosition = Vector2.zero;

		public void Draw(Rect windowRect)
		{
			mBorderRect = new Rect(mLeftWidth, 0, windowRect.width - mLeftWidth - 16 - mRightWidth, windowRect.height - 16);
			mScrollPosition = GUI.BeginScrollView(
				new Rect(mBorderRect.x, mBorderRect.y, mBorderRect.width + 15f, mBorderRect.height + 15f), 
				mScrollPosition, mGraphRect, true, true);

			DrawBackground();
			DrawConnectingLine();
			DrawNodes();

			GUI.EndScrollView();

			//更改鼠标光标
			if (mDragingLeftBorder || mDragingRightBorder || mState == State.Drag)
			{
				Cursor.visible = false;
				DrawMouseIcon("DragIcon");
			}
			else if (mState == State.Shift || mState == State.Ctrl)
			{
				Cursor.visible = false;
				DrawMouseIcon("ShiftIcon");
			}
			else
			{
				Cursor.visible = true;
			}

			HandleEvents();
			CalcGraphRect();
		}

		public void DrawNodes()
		{
			RootNode?.Draw();
			foreach (NodeDesigner node in mDetachedNodes)
			{
				node.Draw();
			}
		}

		//正在连线
		public void DrawConnectingLine()
		{
			if (mSelectedNode == null)
			{
				return;
			}
			Vector2 curPos = Event.current.mousePosition; //MousePosToGraphPos(Event.current.mousePosition);
			if (mState == State.ConnectLeft)
			{
				BTDesignerUtility.DrawConnection(mSelectedNode.LeftPos, curPos);
			}
			if (mState == State.ConnectRight)
			{
				BTDesignerUtility.DrawConnection(mSelectedNode.RightPos, curPos);
			}
		}

		private Vector2 mMousePos = Vector2.zero;
		private Vector2 mSrcOffset = Vector2.zero;
		private bool mLock = true;
		private bool mDragingLeftBorder;
		private bool mDragingRightBorder;

		public void HandleEvents()
		{
			Event e = Event.current;
			switch (e.type)
			{
				case EventType.MouseDown:
					GUI.FocusControl("");
					mMousePos = e.mousePosition;
					if (!BTEditorWindow.windowRect.Contains(mMousePos))
					{
						BTEditorWindow.Instance.CloseSubWin();
					}
					if (BTEditorWindow.windowRect.Contains(mMousePos) && BTEditorWindow.IsShowSubWin)
					{
						break;
					}
					//单击选中
					if (e.button == 0)
					{
						CheckMouseInNode();
					}
					//双击折叠
					if (e.button == 0 && e.clickCount == 2 && mState != State.ConnectLeft && mState != State.ConnectRight)
					{
						mSelectedNode?.Fold();
						CalcGraphRect();
					}
					//右键
					if (e.button == 1)
					{
						//取消选中
						mSelectedNode?.onSelect(false);
						mSelectedNode = null;
						//重新选中
						CheckMouseInNode();

						//右键菜单
						PopMenu();
					}
					if (e.button == 0 && e.mousePosition.x < mLeftWidth + 30 && e.mousePosition.x > mLeftWidth)
					{
						mDragingLeftBorder = true;
					}
					if (e.button == 0 && e.mousePosition.x < mLeftWidth + mBorderRect.width && e.mousePosition.x > mLeftWidth + mBorderRect.width - 30)
					{
						mDragingRightBorder = true;
					}

					break;
				case EventType.MouseUp:
					if (BTEditorWindow.windowRect.Contains(mMousePos) && BTEditorWindow.IsShowSubWin)
					{
						break;
					}
					if (e.button == 0 && e.shift)
					{
						mSelectedNode.Offset = mSrcOffset;
						mSelectedNode.Parent.AutoSort();
						CheckMouseInNode();
					}
					if (e.button == 0)
					{
						CheckMouseInNode();
					}
					mState = State.Normal;
					mDragingLeftBorder = false;
					mDragingRightBorder = false;
					break;
				case EventType.MouseDrag:
					//中键
					if (e.button == 2 || (e.button == 0 && e.alt))
					{
						mScrollPosition.x -= e.delta.x;
						mScrollPosition.y -= e.delta.y;
						mState = State.Normal;
						return;
					}
					if (e.button == 0 && e.shift)
					{
						if (mSelectedNode != null)
						{
							mSrcOffset = mSelectedNode.Offset;
							mSelectedNode.onDrag(e.delta);
							mState = State.Shift;
						}

						return;
					}
					if (e.button == 0 && e.control)
					{
						if (mSelectedNode != null)
						{
							mSrcOffset = mSelectedNode.Offset;
							mSelectedNode.onDrag(e.delta);
							mState = State.Ctrl;
						}
						return;
					}
					if (mDragingLeftBorder)
					{
						mLeftWidth += e.delta.x;
						return;
					}
					if (mDragingRightBorder)
					{
						mRightWidth -= e.delta.x;
						Game.EventSystem.Run(EventIdType.BehaviorTreeRightDesignerDrag, e.delta.x);
						return;
					}

					//左键
					if (e.button == 0 && (e.control || !mLock))
					{
						if (mSelectedNode != null)
						{
							mSelectedNode.onDrag(e.delta);
							mState = State.Drag;
							DrawMouseIcon("DragIcon");
						}
					}
					break;
				case EventType.KeyUp:
					//F1自动排序
					if (e.keyCode == KeyCode.F1)
					{
						RootNode?.AutoSort();
						RootNode.UpdateChildren();
					}
					if (e.keyCode == KeyCode.F2)
					{
						mLock = !mLock;
						if (mLock)
						{
							BehaviorTreeTipsHelper.ShowMessage("节点位置已锁定");
						}
						else
						{
							BehaviorTreeTipsHelper.ShowMessage("节点位置已解锁");
						}
					}
					if (e.keyCode == KeyCode.Delete)
					{
						RemoveNode();
					}
					break;
				case EventType.MouseMove:
					DrawMouseIcon("DragIcon2");
					break;
			}
		}

		public void DrawMouseIcon(string resName)
		{
			GUI.DrawTexture(new Rect(Event.current.mousePosition.x - 10, Event.current.mousePosition.y - 20, 17, 20),
			                BTDesignerUtility.GetTexture(resName));
		}

		public void CheckMouseInNode()
		{
			Vector2 pos = MousePosToGraphPos(Event.current.mousePosition);
			RootNode?.OnMousePos(pos);
			for (int i = 0; i < mDetachedNodes.Count; i++)
			{
				NodeDesigner node = mDetachedNodes[i];
				node.OnMousePos(pos);
			}
		}

		public void CalcGraphRect()
		{
			if (RootNode == null)
			{
				mGraphRect.width = mBorderRect.width;
				mGraphRect.height = mBorderRect.height;
			}
			else
			{
				mGraphRect = RootNode.Size;
				mGraphRect.width += RootNode.Width * 4;
				mGraphRect.height += RootNode.Height;
				if (mGraphRect.width < mBorderRect.width)
				{
					mGraphRect.width = mBorderRect.width;
				}
				if (mGraphRect.height < mBorderRect.height)
				{
					mGraphRect.height = mBorderRect.height;
				}

				RootNode.Pos.x = RootNode.Width;
				RootNode.Pos.y = RootNode.Size.height / 2 + RootNode.Height / 2;
				RootNode.UpdateChildrenPos(RootNode.Offset);
			}
		}

		public Vector2 MousePosToGraphPos(Vector2 mousePos)
		{
			Vector2 graphPos = new Vector2();
			graphPos.x = mousePos.x + mScrollPosition.x - mLeftWidth;
			graphPos.y = mousePos.y + mScrollPosition.y;
			return graphPos;
		}

		public Vector2 CenterPosInBorder()
		{
			Vector2 pos = new Vector2();
			pos.x = mScrollPosition.x + mBorderRect.width / 2;
			pos.y = mScrollPosition.y + mBorderRect.height / 2;
			return pos;
		}

		public void DrawBackground()
		{
			float width = 15f;
			Texture2D bgTex = BTDesignerUtility.GetTexture("Bg");
			GUI.DrawTexture(new Rect(0, 0, mGraphRect.width, mGraphRect.height), bgTex);

			Texture2D lineTex = BTDesignerUtility.GetTexture("BgLine");
			for (int i = 0; i < mGraphRect.width / width; i++)
			{
				GUI.DrawTexture(new Rect(width * i, 0, 1f, mGraphRect.height), lineTex);
			}
			for (int j = 0; j < mGraphRect.height / width; j++)
			{
				GUI.DrawTexture(new Rect(0, width * j, mGraphRect.width, 1f), lineTex);
			}
		}

		public List<string> GetCanRepalceList()
		{
			List<string> result = new List<string>();
			if (mSelectedNode != null)
			{
				BTNodeInfoComponent btNodeInfoComponent = BTEditor.Instance.GetComponent<BTNodeInfoComponent>();
				if (mSelectedNode.NodeData.Proto.classify == NodeClassifyType.Root.ToString() ||
				    BTEditor.Instance.CurTree.Root.Id == mSelectedNode.NodeData.Id)
				{
					
					List<NodeMeta> list = btNodeInfoComponent.GetNodeMetas(NodeClassifyType.Root.ToString());
					foreach (NodeMeta nodeType in list)
					{
						result.Add(nodeType.name);
						// menu.AddItem(new GUIContent(string.Format("{0}/{1}", "替换为", nodeType.name)), false, new GenericMenu.MenuFunction2(this.ChangeNodeType), nodeType.name);
					}
				}
				else
				{
					// NodeChildLimitType nodeChildLimitType = mSelectedNode.NodeData.IsLeaf() ? NodeChildLimitType.All : NodeChildLimitType.WithChild;
					List<NodeMeta> canSubtituteList = btNodeInfoComponent.AllNodeMeta;
					canSubtituteList.Sort(CompareShowName);
					foreach (NodeMeta nodeType in canSubtituteList)
					{
						if (nodeType.classify == NodeClassifyType.Root.ToString())
						{
							continue;
						}
						// menu.AddItem(new GUIContent(string.Format("{0}/{1}", "替换为", nodeType.name)), false, new GenericMenu.MenuFunction2(this.ChangeNodeType), nodeType.name);
						if (mSelectedNode.NodeData.Proto.child_limit <= nodeType.child_limit)
						{
							result.Add(nodeType.name);
						}
					}
				}
			}
			return result;
		}

		public List<string> GetCanCreateList()
		{
			List<string> result = new List<string>();

			BTNodeInfoComponent btNodeInfoComponent = BTEditor.Instance.GetComponent<BTNodeInfoComponent>();
			foreach (string classify in btNodeInfoComponent.GetAllClassify())
			{
				List<NodeMeta> nodeProtoList = btNodeInfoComponent.GetNodeMetas(classify);
				nodeProtoList.Sort(CompareShowName);
				if (classify == NodeClassifyType.Root.ToString())
				{
					continue;
				}
				foreach (NodeMeta node in nodeProtoList)
				{
					if (mSelectedNode != null && mSelectedNode.NodeData.Children.Count < mSelectedNode.NodeData.Proto.child_limit)
					{
						result.Add(node.name);
					}
				}
			}
			return result;
		}

		///菜单相关
		public void PopMenu()
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("创建子节点"), false, this.PopUpCreate);
			menu.AddItem(new GUIContent("替换当前节点"), false, this.PopUpReplace);
			string selectNodeName = BTEditor.Instance.selectNodeName;
			string selectNodeType = BTEditor.Instance.selectNodeType;
			if (mSelectedNode != null && selectNodeName != "")
			{
				if (selectNodeType != NodeClassifyType.Root.ToString() && mSelectedNode.NodeData.Children.Count < mSelectedNode.NodeData.Proto.child_limit)
				{
					menu.AddItem(new GUIContent(string.Format($"新建{selectNodeName}")), false, this.CreateNode);
				}
				else
				{
					menu.AddDisabledItem(new GUIContent("新建"));
				}
				if (mSelectedNode.NodeData.Proto.classify == NodeClassifyType.Root.ToString())
				{
					string menuName = string.Format($"替换成{selectNodeName}");
					if (selectNodeType == NodeClassifyType.Root.ToString())
					{
						menu.AddItem(new GUIContent(menuName), false, this.ChangeNodeType);
					}
					else
					{
						menu.AddDisabledItem(new GUIContent(menuName));
					}
				}
				else
				{
					string menuName = string.Format($"替换成{selectNodeName}");
					NodeClassifyType value = (NodeClassifyType) Enum.Parse(typeof(NodeClassifyType), selectNodeType);
					int count = NodeMetaHelper.NodeTypeCountDict[value];
					if (selectNodeType == NodeClassifyType.Root.ToString() || (count == 0 && mSelectedNode.NodeData.Proto.child_limit > 0))
					{
						menu.AddDisabledItem(new GUIContent(menuName));
					}
					else
					{
						menu.AddItem(new GUIContent(menuName), false, this.ChangeNodeType);
					}
				}
			}
			else
			{
				menu.AddDisabledItem(new GUIContent("新建"));
				menu.AddDisabledItem(new GUIContent("替换"));
			}
			menu.AddItem(new GUIContent("自动排序"), false, this.AutoSort);
			menu.AddItem(new GUIContent("复制"), false, this.CopyNode);
			menu.AddItem(new GUIContent("剪切"), false, this.CutNode);
			menu.AddItem(new GUIContent("粘贴"), false, this.PasteNode);
			menu.AddItem(new GUIContent("删除节点"), false, this.RemoveNode);
			menu.ShowAsContext();
		}

		private void PopUpCreate()
		{
			BTEditorWindow.Instance.ShowSubWin(mMousePos, SubWinType.CreateNode);
		}

		private void PopUpReplace()
		{
			BTEditorWindow.Instance.ShowSubWin(mMousePos, SubWinType.ReplaceNode);
		}

		private static int CompareShowName(NodeMeta nodeType1, NodeMeta nodeType2)
		{
			if (string.IsNullOrEmpty(nodeType1.name) || string.IsNullOrEmpty(nodeType2.name))
			{
				Log.Error("字符串输入参数有误");
			}
			return String.CompareOrdinal(nodeType1.name, nodeType2.name);
		}

		public void AutoSort()
		{
			RootNode?.AutoSort();
		}
		
		private void CreateNode()
		{
			NodeMeta nodeProto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(BTEditor.Instance.selectNodeName);
			BehaviorNodeData nodeData = BTEditor.Instance.CreateNode(nodeProto.name);
			CreateNode(nodeData, MousePosToGraphPos(mMousePos));
		}

		public void CopyNode()
		{
			mCutNode = null;
			mCopyNode = mSelectedNode;
			if (mSelectedNode != null)
			{
				BehaviorTreeTipsHelper.ShowMessage("复制节点" + mSelectedNode.NodeData.Id);
			}
		}

		public void CutNode()
		{
			mCopyNode = null;
			mCutNode = mSelectedNode;
			if (mSelectedNode != null)
			{
				BehaviorTreeTipsHelper.ShowMessage("剪切节点" + mSelectedNode.NodeData.Id);
			}
		}

		public void PasteNode()
		{
			if (mCutNode != null && mCutNode != mSelectedNode)
			{
				ConnectNode(mCutNode, mSelectedNode);
			}
			if (mCopyNode != null && mCopyNode != mSelectedNode)
			{
				BehaviorNodeData data = BTEditor.Instance.CopyNode(mCopyNode.NodeData);
				BTEditor.Instance.ResetTreeId();
				NodeDesigner node = CreateNode(data, Vector2.zero);
				ConnectNode(node, mSelectedNode);
			}
		}

		public void RemoveConnection()
		{
			if (mSelectedNode == null || mSelectedNode == RootNode)
			{
				return;
			}

			foreach (NodeDesigner node in mDetachedNodes)
			{
				if (node == mSelectedNode)
				{
					return;
				}
			}
			mSelectedNode.Parent?.RemoveChild(mSelectedNode);
			mDetachedNodes.Add(mSelectedNode);
		}

		public void RemoveNode()
		{
			if (mSelectedNode == null)
			{
				return;
			}
			if (mSelectedNode.Parent != null)
			{
				mSelectedNode.Parent.RemoveChild(mSelectedNode);
				return;
			}
			mDetachedNodes.Remove(mSelectedNode);
			BTEditor.Instance.ResetTreeId();
		}

		private void ChangeNodeType()
		{
			ChangeNodeType(BTEditor.Instance.selectNodeName);
		}

		//有待优化
		private void ChangeNodeType(object obj)
		{
			string nodeType = (string) obj;
			NodeMeta nodeProto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(nodeType);
			BehaviorNodeData nodeData = BTEditor.Instance.CreateNode(nodeProto.name);
			NodeDesigner oldNode = mSelectedNode;
			NodeDesigner newNode = new NodeDesigner(nodeData);

			if (oldNode == RootNode)
			{
				newNode.NodeData.Id = RootNode.NodeData.Id;
				RootNode = newNode;
				BehaviorTreeData oldTree = BTEditor.Instance.CurTree;
				BehaviorTreeData newTree = new BehaviorTreeData()
				{
					classify = oldTree.classify,
					Root = nodeData
				};
				BTEditor.Instance.CurTree = newTree;
			}
			else
			{
				int idx = oldNode.Parent.Children.IndexOf(oldNode);
				oldNode.Parent.AddChild(newNode, idx);
				oldNode.Parent.RemoveChild(oldNode);
			}

			foreach (NodeDesigner child in oldNode.Children)
			{
				newNode.AddChild(child);
			}
			BTEditor.Instance.ResetTreeId();
			Game.EventSystem.Run(EventIdType.BehaviorTreeAfterChangeNodeType);
		}

		public void onChangeNodeType(params object[] list)
		{
			ChangeNodeType(list[0]);
		}

		/// 节点逻辑相关
		private readonly List<NodeDesigner> mDetachedNodes = new List<NodeDesigner>();

		private NodeDesigner mSelectedNode;
		private NodeDesigner mCopyNode;
		private NodeDesigner mCutNode;

		public NodeDesigner RootNode { get; set; }

		public NodeDesigner CreateNode(BehaviorNodeData nodeData, Vector2 pos)
		{
			NodeDesigner node = new NodeDesigner(nodeData)
			{
				Pos = pos == Vector2.zero ? CenterPosInBorder() : pos
			};
			if (mSelectedNode != null)
			{
				mSelectedNode.AddChild(node);
				mSelectedNode.AutoSort();
			}
			else
			{
				mDetachedNodes.Add(node);
			}
			BTEditor.Instance.ResetTreeId();
			Game.EventSystem.Run(EventIdType.BehaviorTreeCreateNode, node);
			return node;
		}

		/// 事件相关
		public NodeDesigner onCreateTree(params object[] list)
		{
			if (BTEditor.Instance.CurTree == null)
			{
				Log.Error($"CurTree can not be null");
				return null;
			}
			RootNode = new NodeDesigner(BTEditor.Instance.CurTree.Root);
			CalcGraphRect();
			return RootNode;
		}

		public void onSelectNode(params object[] list)
		{
			mSelectedNode?.onSelect(false);
			mSelectedNode = (NodeDesigner) list[1];
			mSelectedNode.onSelect(true);
		}

		public void onCreateNode(params object[] list)
		{
			string nodeName = (string) list[0];
			Vector2 pos = (Vector2) list[1];

			NodeMeta nodeProto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(nodeName);
			BehaviorNodeData nodeData = BTEditor.Instance.CreateNode(nodeProto.name);
			CreateNode(nodeData, pos);
		}

		public void onStartConnect(params object[] list)
		{
			NodeDesigner node = (NodeDesigner) list[0];
			State state = (State) list[1];
			if (node == RootNode && state == State.ConnectLeft) //根节点不让左连接
			{
				return;
			}
			mState = state;
		}

		public void onMouseInNode(params object[] list)
		{
			NodeDesigner dstNode = (NodeDesigner) list[1];
			if (dstNode == mSelectedNode || dstNode == null)
			{
				return;
			}
			switch (mState)
			{
				case State.Normal:
					ClickNode(dstNode);
					break;
				case State.Drag: break;
				case State.Shift:
					ShiftNode(dstNode);
					break;
				case State.Ctrl:
					MoveNode(dstNode);
					break;
				case State.ConnectLeft:
					ConnectNode(mSelectedNode, dstNode);
					break;
				case State.ConnectRight:
					ConnectNode(dstNode, mSelectedNode);
					break;
			}
		}

		//src接到dst的子节点
		public void ConnectNode(NodeDesigner srcNode, NodeDesigner dstNode)
		{
			if (srcNode == null || dstNode == null || !dstNode.NodeData.CanAddChild())
			{
				return;
			}
			if (srcNode.FindChild(dstNode))
			{
				//src有dst这个子节点不让连，避免死循环
				//Log.Info("found child");
				return;
			}

			NodeDesigner parent = srcNode.Parent;
			if (parent != null)
			{
				parent.RemoveChild(srcNode);
			}
			dstNode.AddChild(srcNode);
			dstNode.AutoSort();

			mDetachedNodes.Remove(srcNode);
		}

		public void ClickNode(NodeDesigner dstNode)
		{
			Game.EventSystem.Run(EventIdType.BehaviorTreeClickNode, dstNode);
		}

		public void ShiftNode(NodeDesigner dstNode)
		{
			Log.Info("shift node");

			if (mSelectedNode == null)
			{
				return;
			}
			NodeDesigner node1 = dstNode;
			NodeDesigner node2 = mSelectedNode;
			NodeDesigner parent1 = node1.Parent;
			NodeDesigner parent2 = node2.Parent;
			//根节点不可交换
			if (parent2 == null)
			{
				return;
			}
			//同父交换位置
			if (parent1 == parent2)
			{
				parent1.RemoveChild(node2);
				int idx = parent1.Children.IndexOf(node1);
				parent1.AddChild(node2, idx);
				parent1.AutoSort();
				//BTEditor.GetInstance().ResetTreeId();
			}

			//             //不同父，插到node1的子节点
			//             if (!node1.NodeData.CanAddChild())
			//                 return;
			// 
			//             parent2.RemoveChild(node2);
			//             parent2.AutoSort();
			//             node1.AddChild(node2);
			//             node1.AutoSort();
			//             BTEditor.GetInstance().ResetTreeId();
		}

		public void MoveNode(NodeDesigner dstNode)
		{
			Log.Info("Move  node");

			if (mSelectedNode == null)
			{
				return;
			}
			NodeDesigner node1 = dstNode;
			NodeDesigner node2 = mSelectedNode;
			NodeDesigner parent1 = node1.Parent;
			NodeDesigner parent2 = node2.Parent;
			//根节点不可交换
			if (parent2 == null)
			{
				return;
			}

			//不同父，插到node1的子节点
			if (!node1.NodeData.CanAddChild())
			{
				return;
			}

			parent2.RemoveChild(node2);
			parent2.AutoSort();
			node1.AddChild(node2);
			node1.AutoSort();
			//BTEditor.GetInstance().ResetTreeId();
		}
	}
}