using System;
using System.Collections.Generic;
using Base;

namespace Model
{
	public class BehaviorTree
	{
		private readonly Node node;
		public BehaviorTreeConfig behaviorTreeConfig { get; set; }
		public Scene Scene { get; }

		public string Discription
		{
			get
			{
				return this.node.Description;
			}
		}

		public BehaviorTree(Scene scene, Node node)
		{
			this.Scene = scene;
			this.node = node;
		}

		public bool Run(BTEnv env)
		{
			try
			{
				bool ret = this.node.DoRun(this, env);
				List<long> pathList = env.Get<List<long>>(BTEnvKey.NodePath);
				Game.Scene.GetComponent<EventComponent>().Run(EventIdType.BehaviorTreeRunTreeEvent, this, pathList);
				return ret;
			}
			catch (Exception e)
			{
				string source = env.Get<string>(BTEnvKey.BTSource) ?? "";
				Log.Error($"树运行出错, 树名: {this.Discription} 来源: {source}" + e);
				return false;
			}
		}
	}
}