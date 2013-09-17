using System;
using System.Collections.Generic;

namespace BehaviorTree
{
	public class BehaviorTreeFactory
	{
		private readonly Dictionary<string, Func<Config, Node>> dictionary =
			new Dictionary<string, Func<Config, Node>>();

		public BehaviorTreeFactory()
		{
			this.dictionary.Add("selector", config => new Selector(config));
			this.dictionary.Add("sequence", config => new Sequence(config));
		}

		public void Register(string name, Func<Config, Node> action)
		{
			this.dictionary.Add(name, action);
		}

		private Node CreateNode(Config config)
		{
			if (!this.dictionary.ContainsKey(config.Name))
			{
				throw new KeyNotFoundException(string.Format("CreateNode cannot found: {0}", config.Name));
			}
			return this.dictionary[config.Name](config);
		}

		public BehaviorTree CreateTree(Config config)
		{
			var node = this.CreateNode(config);
			if (config.SubConfigs == null)
			{
				return new BehaviorTree(node);
			}

			foreach (var subConfig in config.SubConfigs)
			{
				var subNode = this.CreateNode(subConfig);
				node.AddChild(subNode);
			}
			return new BehaviorTree(node);
		}
	}
}
