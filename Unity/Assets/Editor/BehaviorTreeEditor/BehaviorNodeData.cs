using System.Collections.Generic;
using Model;
using UnityEngine;

namespace MyEditor
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
		public int nodeId;

		public string name;

		public string describe = "";

		public List<BehaviorNodeData> children = new List<BehaviorNodeData>();

		public BehaviorTreeArgsDict args_dict = new BehaviorTreeArgsDict();

		/// <summary>
		///  
		/// </summary>
		public string error = "";

		private string mClassify = "";

		public NodeDesigner NodeDesigner { get; set; }

		public Vector2 Pos;
		public DebugState NodeDeubgState { get; set; }
		public string time;

		public ClientNodeTypeProto Proto { get; set; }

		public List<BehaviorNodeData> Children
		{
			get
			{
				return children;
			}
		}

		public BehaviorNodeData Parent { get; set; }

		public string Classify
		{
			get
			{
				return mClassify;
			}
			set
			{
				mClassify = value;
			}
		}

		public BehaviorNodeData(string proto_name)
		{
			name = proto_name;
			this.Proto = BehaviorManager.Instance.GetNodeTypeProto(proto_name);
			if (this.Proto == null)
			{
				this.Proto = BehaviorManager.Instance.GetNodeTypeProto("Unknow");
				return;
			}
			mClassify = this.Proto.classify;

			foreach (NodeFieldDesc args_desc in this.Proto.new_args_desc)
			{
				args_dict.SetKeyValueComp(args_desc.type, args_desc.name, args_desc.value);
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
			this.nodeId = BehaviorManager.Instance.AutoNodeId();
			foreach (BehaviorNodeData child in children)
			{
				child.ResetId();
			}
		}
	}
}