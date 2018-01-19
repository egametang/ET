using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
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
		}

		protected Disposer(long id)
		{
			this.Id = id;
		}

		public virtual void Dispose()
		{
			this.Id = 0;
			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
			}
		}
	}
}