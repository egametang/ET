using System;
using ETModel;
using MongoDB.Bson.Serialization.Attributes;

namespace ETEditor
{
	[BsonIgnoreExtraElements]
	[Serializable]
	public class BehaviorTreeData
	{
		[BsonElement, BsonIgnoreIfNull]
		public BehaviorNodeData BehaviorNodeData;

		[BsonElement]
		public string classify = "";

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