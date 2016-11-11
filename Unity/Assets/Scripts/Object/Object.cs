using System;
using System.ComponentModel;
using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class Object: IDisposable, ISupportInitialize, ICloneable
	{
		[BsonId]
		[BsonIgnoreIfDefault]
		public long Id { get; private set; }

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
			return MongoHelper.ToJson(this);
		}

		public virtual void Dispose()
		{
			this.Id = 0;
		}

		public object Clone()
		{
			return MongoHelper.FromJson(this.GetType(), this.ToString());
		}
	}
}