using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonIgnoreExtraElements]
	public abstract partial class Component: Disposer
	{
		[BsonIgnore]
		public Disposer Parent { get; set; }

		public T GetParent<T>() where T : Disposer
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