using System.Collections.Generic;
using ETModel;
using UnityEngine;

namespace ETEditor
{
	public enum DebugState
	{
		Disable,
		True,
		False,
		Error,
		Normal
	}

	public class BehaviorNodeData
	{
		public int Id;

		public string Name;

		public string Desc = "";

		public string Classify { get; set; } = "";

		public BehaviorNodeData Parent { get; set; }

		public List<BehaviorNodeData> children = new List<BehaviorNodeData>();

		public BehaviorTreeArgsDict Args = new BehaviorTreeArgsDict();

		/// <summary>
		///  
		/// </summary>
		public string Error = "";

		public Vector2 Pos;
		public DebugState NodeDeubgState { get; set; }
		public string time;

		public NodeMeta Proto { get; set; }

		public List<BehaviorNodeData> Children
		{
			get
			{
				return children;
			}
		}

		public BehaviorNodeData(string proto_name)
		{
			this.Name = proto_name;
			this.Proto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta(proto_name);
			if (this.Proto == null)
			{
				this.Proto = BTEditor.Instance.GetComponent<BTNodeInfoComponent>().GetNodeMeta("Unknow");
				return;
			}
			this.Classify = this.Proto.classify;

			foreach (NodeFieldDesc args_desc in this.Proto.new_args_desc)
			{
				this.Args.SetKeyValueComp(args_desc.name, args_desc.value);
			}

			foreach (BehaviorNodeData child in children)
			{
				AddChild(child);
			}
		}

		public BehaviorNodeData()
		{
		}

		//默认添加到末尾
		public void AddChild(BehaviorNodeData node, int index = -1)
		{
			index = index == -1? this.Children.Count : index;
			children.Insert(index, node);
			node.Parent = this;
		}

		public BehaviorNodeData RemoveChild(BehaviorNodeData node)
		{
			node.Parent = null;
			children.Remove(node);
			return node;
		}

		public bool IsLeaf()
		{
			return children.Count == 0;
		}

		public bool IsRoot()
		{
			return Parent == null;
		}

		public bool CanAddChild()
		{
			return children.Count < Proto.child_limit;
		}

		public void ResetId()
		{
			this.Id = BTEditor.Instance.AutoNodeId();
			foreach (BehaviorNodeData child in children)
			{
				child.ResetId();
			}
		}
	}
}