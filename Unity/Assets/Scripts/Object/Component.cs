using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	[BsonKnownTypes(typeof(AConfigComponent))]
	public abstract class Component : Object, IDisposable
	{
		[BsonIgnore]
		public Entity Owner { get; set; }

		public T GetOwner<T>() where T: Entity
		{
			return this.Owner as T;
		}

		protected Component()
		{
			ObjectManager.Instance.Add(this);
		}

		protected Component(long id): base(id)
		{
			ObjectManager.Instance.Add(this);
		}

		protected T GetComponent<T>() where T: Component
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
			
			ObjectManager.Instance.Remove(this);
		}
	}
}