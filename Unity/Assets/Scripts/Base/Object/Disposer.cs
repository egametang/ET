using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(Entity))]
	[BsonKnownTypes(typeof(Component))]
	public abstract class Disposer : Object, IDisposable
	{
		[BsonIgnoreIfDefault]
		[BsonDefaultValue(1L)]
		[BsonElement]
		[BsonId]
		public long Id { get; set; }

		[BsonIgnore]
		public bool IsFromPool { get; set; }
	
		protected Disposer()
		{
			this.Id = IdGenerater.GenerateId();
			ObjectEvents.Instance.Add(this);
		}

		protected Disposer(long id)
		{
			this.Id = id;
			ObjectEvents.Instance.Add(this);
		}

		public virtual void Dispose()
		{
			this.Id = 0;
			if (this.IsFromPool)
			{
				ObjectPool.Instance.Recycle(this);
			}
		}
	}
}