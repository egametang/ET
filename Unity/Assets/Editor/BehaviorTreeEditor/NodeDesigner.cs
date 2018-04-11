using System.Collections.Generic;
using ETModel;
using UnityEngine;

namespace ETEditor
{
	public class NodeDesigner
	{
		private Texture2D mBoxHighLight;
		private Texture2D mBoxSelectHighLight;
		private bool isSelected;

		public NodeDesignerProto DesignerData { get; set; }

		public NodeDesigner(BehaviorNodeData data)
		{
			NodeData = data;
			DesignerData = new NodeDesignerProto();
			Init();
			UpdateChildren();
		}

		public List<NodeDesigner> Children { get; } = new List<NodeDesigner>();

		public NodeDesigner Parent { get; set; }

		public void UpdateChildren()
		{
			if (!DesignerData.fold)
			{
				this.Children.Clear();
				foreach (BehaviorNodeData childData in NodeData.Children)
				{
					NodeDesigner child = new NodeDesigner(childData);
					this.Children.Add(child);
					child.Parent = this;
				}
			}
			else
			{
				this.Children.Clear();
				return;
			}
			UpdateChildrenPos(this.Offset);
		}

		public BehaviorNodeData NodeData { get; set; }

		//坐标位置相关
		public float Width;

		public float Height;
		public Vector2 Pos = Vector2.zero; //中心点

		public Vector2 Offset
		{
			get
			{
				return new Vector2(DesignerData.x, DesignerData.y);
			}
			set
			{
				DesignerData.x = value.x;
				DesignerData.y = value.y;
			}
		}

		public Vector2 LeftPos //左边坐标
		{
			get
			{
				return new Vector2(Pos.x - Width / 2, Pos.y);
			}
		}

		public Vector2 RightPos //右边坐标
		{
			get
			{
				return new Vector2(Pos.x + Width / 2, Pos.y);
			}
		}

		public Rect Size;
		
		private Texture2D mBoxTex;
		private Texture2D mLeftConnectTex;
		private Texture2D mRightConnectTex;

		public void Init()
		{
			NodeData.Proto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(NodeData.Name);
			string[] arr = NodeData.Proto.style.Split('/');
			string style = arr.Length > 0? arr[0] : "";
			if (style == "")
			{
				switch (NodeData.Classify)
				{
					case "装饰节点":
						style = "green";
						break;
					case "复合节点":
						style = "blue";
						break;
					case "条件节点":
						style = "orange";
						break;
				}
				switch (NodeData.Name)
				{
					case "Sequence":
						style = "blue";
						break;
					case "Selector":
						style = "green";
						break;
					default:
						style = "default";
						break;
				}
			}
			mBoxTex = BTDesignerUtility.GetTexture(style);
			mBoxHighLight = BTDesignerUtility.GetTexture("HighLight");
			mBoxSelectHighLight = BTDesignerUtility.GetTexture("SelectHighLight");

			if (mBoxTex == null)
			{
				mBoxTex = BTDesignerUtility.GetTexture("default");
			}
			if (mBoxTex == null)
			{
				Log.Info("mBoxTex null " + NodeData.Id);
			}
			Width = mBoxTex.width / 2;
			Height = mBoxTex.height / 2;

			mLeftConnectTex = BTDesignerUtility.GetTexture("LeftConnect");
			mRightConnectTex = BTDesignerUtility.GetTexture("RightConnect");
		}

		public void Draw()
		{
			foreach (NodeDesigner child in this.Children)
			{
				//先画子节点，让线条在最低层
				BTDesignerUtility.DrawConnection(this.RightPos, child.LeftPos);
				child.Draw();
			}

			//左链接
			Texture2D tex = mLeftConnectTex;
			Rect rect = new Rect(Pos.x - Width / 2 - tex.width / 6, Pos.y - tex.height / 4, tex.width / 2, tex.height / 2);
			GUI.DrawTexture(rect, tex);
			//右链接
			if (NodeData.Proto.child_limit > 0)
			{
				tex = mRightConnectTex;
				rect = new Rect(Pos.x + Width / 2 - tex.width / 6 * 2, Pos.y - tex.height / 4.1f, tex.width / 2, tex.height / 2);
				GUI.DrawTexture(rect, tex);

				if (NodeData.Children.Count > 0)
				{
					if (DesignerData.fold)
					{
						GUI.Label(new Rect(Pos.x + Width / 2 - 5f, Pos.y - 9f, tex.width, tex.height), "+");
					}
					else
					{
						GUI.Label(new Rect(Pos.x + Width / 2 - 8f, Pos.y - 9f, tex.width, tex.height), "—");
					}
				}
			}

			rect = new Rect(Pos.x - Width / 2, Pos.y - Height / 2, Width, Height);
			GUI.DrawTexture(rect, mBoxTex);
			if (isSelected)
			{
				GUI.DrawTexture(rect, mBoxSelectHighLight);
			}
			else if (BTEditor.Instance.IsHighLight(this.NodeData))
			{
				GUI.DrawTexture(rect, mBoxHighLight);
			}

			GUIStyle style = new GUIStyle();
			style.normal.background = null;
			style.normal.textColor = new Color(1, 1, 1);
			style.fontSize = 15;
			if (!this.isSelected)
			{
				style.clipping = TextClipping.Clip;
			}
			GUI.Label(new Rect(Pos.x - Width / 2 + 5f, Pos.y - Height / 3, Width - 10f, Height / 2), NodeData.Proto.name, style);

			style.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
			style.fontSize = 12;
			style.wordWrap = true;
			string deprecatedDesc = NodeData.Proto.isDeprecated? $"({NodeData.Proto.deprecatedDesc})" : "";
			GUI.Label(new Rect(Pos.x - Width / 2 + 6f, Pos.y, Width - 12f, Height / 2.1f), NodeData.Desc + deprecatedDesc, style);

			tex = null;
			switch (NodeData.NodeDeubgState)
			{
				case DebugState.True:
					tex = BTDesignerUtility.GetTexture("Execute");
					break;
				//                 case DebugState.False:
				//                     tex = BTDesignerUtility.GetTexture("False");
				//                     break;
				//                 case DebugState.Error:
				//                     tex = BTDesignerUtility.GetTexture("Error");
				//                     break;
			}
			if (tex != null)
			{
				GUI.DrawTexture(rect, tex);
				//                 string time = System.DateTime.Now.ToString();
				//                 GUI.Label(new Rect(Pos.x + Width / 2, Pos.y -20, Width - 12f, Height / 2.1f), time, style);
			}
		}

		public void AddChild(NodeDesigner child, int index = -1)
		{
			index = index == -1? this.Children.Count : index;
			this.Children.Insert(index, child);
			child.Parent = this;
			NodeData.AddChild(child.NodeData, index);
		}

		public void RemoveChild(NodeDesigner child)
		{
			this.Children.Remove(child);
			child.Parent = null;
			NodeData.RemoveChild(child.NodeData);
		}

		public void UpdateSize()
		{
			Size.width = Width * 1.5f;
			Size.height = Height;
			if (this.Children.Count == 0)
			{
				return;
			}
			foreach (NodeDesigner child in this.Children)
			{
				child.UpdateSize();
			}

			float max = 0;
			foreach (NodeDesigner child in this.Children)
			{
				max = max < child.Size.width? child.Size.width : max;
			}
			Size.width += max;

			Size.height = 0;
			foreach (NodeDesigner child in this.Children)
			{
				Size.height += child.Size.height;
			}
		}

		public void UpdateChildrenPos(Vector2 offset)
		{
			UpdateSize();
			float y = this.Pos.y - this.Size.height / 2;
			foreach (NodeDesigner child in this.Children)
			{
				child.Pos.x = this.Pos.x + Width * 1.5f + child.Offset.x;
				child.Pos.y = y + child.Size.height / 2 + child.Offset.y;
				y += child.Size.height;
				child.UpdateChildrenPos(offset);
			}
		}

		public void OnMousePos(Vector2 mouse)
		{
			//判断是否被点中
			if (!isSelected && mouse.x > Pos.x - Width / 2 - 30f && mouse.x < Pos.x + Width / 2 + 30f && mouse.y > Pos.y - Height / 2 &&
			    mouse.y < Pos.y + Height / 2)
			{
				Game.EventSystem.Run(EventIdType.BehaviorTreeMouseInNode, NodeData, this);
			}
			//并判断是否点中了连线柄
			if (mouse.x > LeftPos.x - 30f && mouse.x < LeftPos.x + 10f && mouse.y > LeftPos.y - 30f && mouse.y < LeftPos.y + 30f)
			{
				Game.EventSystem.Run(EventIdType.BehaviorTreeConnectState, this, State.ConnectLeft);
			}

			if (mouse.x > RightPos.x - 10f && mouse.x < RightPos.x + 30f && mouse.y > RightPos.y - 30f && mouse.y < RightPos.y + 30f && NodeData.CanAddChild())
			{
				Game.EventSystem.Run(EventIdType.BehaviorTreeConnectState, this, State.ConnectRight);
			}

			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].OnMousePos(mouse);
			}
		}

		public void onSelect(bool flag)
		{
			isSelected = flag;
			if (flag)
			{
				BTEditor.Instance.SelectNode(this.NodeData);
			}
		}

		public void onDrag(Vector2 delta)
		{
			Offset = new Vector2(Offset.x + delta.x, Offset.y + delta.y);
			Pos.x += delta.x;
			Pos.y += delta.y;
			UpdateChildrenPos(this.Offset);
		}

		//折叠
		public void Fold()
		{
			DesignerData.fold = !DesignerData.fold;
			UpdateChildren();
		}

		//自动排序
		public void AutoSort()
		{
			this.Offset = Vector2.zero;
			foreach (NodeDesigner child in this.Children)
			{
				child.AutoSort();
			}
			UpdateChildrenPos(this.Offset);
		}

		public bool FindChild(NodeDesigner dstNode)
		{
			if (this == dstNode)
			{
				return true;
			}
			foreach (NodeDesigner child in this.Children)
			{
				if (child.FindChild(dstNode))
				{
					return true;
				}
			}
			return false;
		}
	}
}