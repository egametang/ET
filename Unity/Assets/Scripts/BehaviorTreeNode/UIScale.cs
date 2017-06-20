using Base;
using UnityEngine;

namespace Model
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
		[NodeInput("传入的UI参数", typeof(UI))]
		private string uiKey;

		[NodeField("缩放倍数")]
		private float scale;

		[NodeField("111")]
		private AAAA aaaa;

		public UIScale(NodeProto nodeProto): base(nodeProto)
		{
		}

		protected override bool Run(BehaviorTree behaviorTree, BTEnv env)
		{
			UI ui = env.Get<UI>(this.uiKey);
			ui.GameObject.transform.localScale = new Vector3(this.scale, this.scale, this.scale);
			return true;
		}
	}
}