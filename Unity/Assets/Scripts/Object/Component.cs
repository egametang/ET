using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	public abstract class Component: Disposer
	{
		[BsonIgnore]
		public Entity Owner { get; set; }

		public T GetOwner<T>() where T : Entity
		{
			return this.Owner as T;
		}

		protected Component()
		{
		}

		protected Component(long id): base(id)
		{
		}

		public T GetComponent<T>() where T : Component
		{
			return this.Owner.GetComponent<T>();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.Owner.RemoveComponent(this.GetType());
		}
	}
}