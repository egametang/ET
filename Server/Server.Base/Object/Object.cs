using System;
using System.ComponentModel;
using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	public abstract class Object: IDisposable, ISupportInitialize
	{
		public static ObjectManager ObjectManager = new ObjectManager();

		[BsonId]
		public long Id { get; private set; }

		protected Object()
		{
			Id = IdGenerater.GenerateId();
		}

		protected Object(long id)
		{
			this.Id = id;
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