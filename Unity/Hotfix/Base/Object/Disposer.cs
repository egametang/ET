using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	public abstract class Disposer : Object, IDisposable2
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