using System;
using System.ComponentModel;
using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class Object: ISupportInitialize, ICloneable
	{
		[BsonId]
		[BsonIgnoreIfDefault]
		public long Id { get; protected set; }

		protected Object()
		{
			Id = IdGenerater.GenerateId();
		}

		protected Object(long id)
		{
			this.Id = id;
		}

		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}

		public override string ToString()
		{
			return this.ToJson();
		}

		public string ToJson()
		{
			return MongoHelper.ToJson(this);
		}

		public byte[] ToBson()
		{
			return MongoHelper.ToBson(this);
		}

		public object Clone()
		{
			return MongoHelper.FromBson(this.GetType(), this.ToBson());
		}
	}
}