using MongoDB.Bson.Serialization.Attributes;

namespace Hotfix
{
	public abstract class Component : Disposer
	{
		public long Id { get; set; }

		[BsonIgnore]
		public Component Parent { get; set; }

		public T GetParent<T>() where T : Entity
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