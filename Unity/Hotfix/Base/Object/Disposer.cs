using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	public abstract class Disposer : Object, IDisposable2
	{
		[BsonIgnoreIfDefault]
		[BsonDefaultValue(1L)]
		[BsonElement]
		public long Id { get; set; }

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