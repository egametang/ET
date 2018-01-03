using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public abstract partial class Component: Disposer
	{
		[BsonIgnore]
		public Entity Parent { get; set; }

		public T GetParent<T>() where T : Entity
		{
			return this.Parent as T;
		}

		protected Component()
		{
			this.Id = 1;
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}