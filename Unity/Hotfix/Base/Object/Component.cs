using MongoDB.Bson.Serialization.Attributes;

namespace ETHotfix
{
	public abstract class Component : Disposer
	{
		public long Id { get; set; }

		[BsonIgnore]
		public Component Parent { get; set; }

		public T GetParent<T>() where T : Component
		{
			return this.Parent as T;
		}

		public Entity Entity
		{
			get
			{
				return this.Parent as Entity;
			}
		}
		
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}