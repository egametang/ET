using UnityEngine;

namespace ETModel
{
	[Node(NodeClassifyType.Action, "创建UI特效")]
	public class CreateUIEffect : Node
	{
		[NodeField("特效")]
		private GameObject effect;

		public CreateUIEffect(NodeProto nodeProto): base(nodeProto)
		{
		}

		protected override bool Run(BehaviorTree behaviorTree, BTEnv env)
		{
			Log.Debug($"创建UI特效: {this.effect.name}");
			return true;
		}
	}
}