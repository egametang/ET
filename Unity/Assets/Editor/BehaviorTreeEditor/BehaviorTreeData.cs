using System;
using Model;
using MongoDB.Bson.Serialization.Attributes;

namespace MyEditor
{
	[BsonIgnoreExtraElements]
	[Serializable]
	public class BehaviorTreeData: AConfig
	{
		[BsonElement, BsonIgnoreIfNull]
		public BehaviorNodeData BehaviorNodeData;

		[BsonElement]
		public string classify = "";

		public BehaviorTreeData(): base(EntityType.Config)
		{
		}

		public BehaviorTreeData(long id): base(id, EntityType.Config)
		{
		}

		[BsonIgnore]
		public BehaviorNodeData Root
		{
			get
			{
				return this.BehaviorNodeData;
			}
			set
			{
				this.BehaviorNodeData = value;
			}
		}
	}
}