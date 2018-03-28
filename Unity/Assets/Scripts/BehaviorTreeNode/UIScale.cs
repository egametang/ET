using UnityEngine;

namespace ETModel
{
	public enum AAAA
	{
		BBBB,
		CCCC,
		DDDD
	}

	[Node(NodeClassifyType.Action, "将UI缩放")]
	public class UIScale : Node
	{
		[NodeField("缩放倍数")]
		private float scale;

		[NodeField("111")]
		private AAAA aaaa;

		public UIScale(NodeProto nodeProto): base(nodeProto)
		{
		}

		protected override bool Run(BehaviorTree behaviorTree, BTEnv env)
		{
			return true;
		}
	}
}