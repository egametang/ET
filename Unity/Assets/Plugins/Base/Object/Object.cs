using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	public abstract class Object: IDisposable, ISupportInitialize
	{
		[BsonIgnore]
		public static ObjectManager ObjectManager = new ObjectManager();

		[BsonId]
		public long Id { get; private set; }

		protected Object()
		{
			Id = IdGenerater.GenerateId();
			ObjectManager.Add(this);
		}

		protected Object(long id)
		{
			this.Id = id;
			ObjectManager.Add(this);
		}

		public bool IsDisposed()
		{
			return this.Id == 0;
		}

		public virtual void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			ObjectManager.Remove(this.Id);

			this.Id = 0;
		}

		public virtual void BeginInit()
		{
		}

		public virtual void EndInit()
		{
		}
	}
}