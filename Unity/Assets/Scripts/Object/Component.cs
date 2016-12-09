using Base;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	public abstract class Component : Disposer
	{
		[BsonIgnore]
		public Entity Owner { get; set; }

		public T GetOwner<T>() where T: Entity
		{
			return this.Owner as T;
		}

		protected Component()
		{
			Game.EntityEventManager.Add(this);
		}

		protected Component(long id): base(id)
		{
			Game.EntityEventManager.Add(this);
		}

		public T GetComponent<T>() where T: Component
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

			Game.EntityEventManager.Remove(this);
		}

		public override void EndInit()
		{
			base.EndInit();

			Game.EntityEventManager.Add(this);
		}
	}
}