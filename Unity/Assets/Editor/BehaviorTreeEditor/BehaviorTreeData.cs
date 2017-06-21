using System;
using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace MyEditor
{
	[BsonIgnoreExtraElements]
	[Serializable]
	public class BehaviorTreeData
	{
		public long Id;

		[BsonElement, BsonIgnoreIfNull]
		public BehaviorNodeData BehaviorNodeData;

		[BsonElement]
		public string classify = "";

		public BehaviorTreeData()
		{
			this.Id = IdGenerater.GenerateId();
		}

		public BehaviorTreeData(long id)
		{
			this.Id = id;
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