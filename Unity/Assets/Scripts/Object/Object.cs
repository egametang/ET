using System;
using System.ComponentModel;
using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class Object: IDisposable, ISupportInitialize
	{
		[BsonId]
		public long Id { get; private set; }

		protected Object()
		{
			Id = IdGenerater.GenerateId();
			ObjectManager.Instance.Add(this);
		}

		protected Object(long id)
		{
			this.Id = id;
			ObjectManager.Instance.Add(this);
		}

		public virtual void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			ObjectManager.Instance.Remove(this);

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