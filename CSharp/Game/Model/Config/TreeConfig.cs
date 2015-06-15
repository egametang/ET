using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public class NodeConfig
	{
		public int Id { get; set; }
		public NodeType Type { get; set; }
		public List<string> Args { get; set; }
		public List<NodeConfig> Children { get; set; }
	}

	public class TreeConfig : AConfig
	{
		[BsonElement, BsonIgnoreIfNull]
		private NodeConfig root;

		[BsonIgnore]
		public NodeConfig Root
		{
			get
			{
				return this.root;
			}
			set
			{
				this.root = value;
			}
		}
	}
}
