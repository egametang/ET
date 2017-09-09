using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class Disposer : Object, IDisposable
	{
		[BsonIgnore]
		public abstract long Id { get; set; }
		
		protected Disposer()
		{
			ObjectEvents.Instance.Add(this);
		}

		public virtual void Dispose()
		{
			this.Id = 0;
		}
	}
}