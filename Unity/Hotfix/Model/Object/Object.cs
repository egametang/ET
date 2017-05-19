using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class Object: IDisposable
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

		public virtual void Dispose()
		{
		}

		public object Clone()
		{
			return MongoHelper.FromBson(this.GetType(), this.ToBson());
		}
	}
}