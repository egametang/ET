using System.Collections.Generic;
using System.Linq;
using ETModel;

namespace ETEditor
{
	[ObjectSystem]
	public class BtNodeInfoComponentSystem : AwakeSystem<BTNodeInfoComponent>
	{
		public override void Awake(BTNodeInfoComponent self)
		{
			self.Awake();
		}
	}

	public class BTNodeInfoComponent : Component
	{
		private Dictionary<string, NodeMeta> nameNodeMetas = new Dictionary<string, NodeMeta>(); //节点类型 name索引
		private Dictionary<string, List<NodeMeta>> classifyNodeMetas { get; } = new Dictionary<string, List<NodeMeta>>();

		public void Awake()
		{
			LoadNodeTypeProto();
		}

		public void LoadNodeTypeProto()
		{
			this.nameNodeMetas = NodeMetaHelper.ExportToDict();

			foreach (NodeMeta nodeType in this.nameNodeMetas.Values)
			{
				if (nodeType.isDeprecated)
				{
					continue;
				}
				string classify = nodeType.classify;
				if (classify == "")
				{
					classify = "未分类";
				}
				if (!this.classifyNodeMetas.ContainsKey(classify))
				{
					this.classifyNodeMetas.Add(classify, new List<NodeMeta>());
				}
				this.classifyNodeMetas[classify].Add(nodeType);
			}
		}

		public List<NodeMeta> GetNodeMetas(string classify)
		{
			List<NodeMeta> nodeMetas = null;
			this.classifyNodeMetas.TryGetValue(classify, out nodeMetas);
			return nodeMetas;
		}

		public string[] GetAllClassify()
		{
			return this.classifyNodeMetas.Keys.ToArray();
		}

		public NodeMeta GetNodeMeta(string nodeName)
		{
			NodeMeta nodeMeta = null;
			this.nameNodeMetas.TryGetValue(nodeName, out nodeMeta);
			return nodeMeta;
		}

		public bool ContainsKey(string nodeName)
		{
			return this.nameNodeMetas.ContainsKey(nodeName);
		}

		public List<NodeMeta> AllNodeMeta
		{
			get
			{
				return this.nameNodeMetas.Values.ToList();
			}
		}
	}
}
