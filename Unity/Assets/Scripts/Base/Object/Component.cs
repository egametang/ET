using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	public abstract class Component: Disposer
	{
		[BsonIgnore]
		public Entity Entity { get; set; }

		public T GetEntity<T>() where T : Entity
		{
			return this.Entity as T;
		}

		protected Component()
		{
		}

		protected Component(long id): base(id)
		{
		}

		public T GetComponent<T>() where T : Component
		{
			return this.Entity.GetComponent<T>();
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();

			this.Entity?.RemoveComponent(this.GetType());
		}
	}
}