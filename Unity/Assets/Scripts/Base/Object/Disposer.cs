using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(Component))]
	public abstract class Disposer : Object, IDisposable
	{
		[BsonIgnore]
		public bool IsFromPool { get; set; }

		[BsonIgnore]
		public bool IsDisposed { get; set; }
	
		public virtual void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			this.IsDisposed = true;

			if (this.IsFromPool)
			{
				Game.ObjectPool.Recycle(this);
			}
		}
	}
}