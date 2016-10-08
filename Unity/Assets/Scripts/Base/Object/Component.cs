using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	/// <summary>
	/// Component的Id与Owner Entity Id一样
	/// </summary>
	public abstract class Component: Object
	{
		private Entity owner;
		
		public T GetOwner<T>() where T: Entity
		{
			return this.owner as T;
		}

		public void SetOwner(Entity entity)
		{
			this.owner = entity;
		}

		protected Component()
		{
			ObjectManager.Add(this);
		}

		protected Component(long id): base(id)
		{
			ObjectManager.Add(this);
		}

		public override void Dispose()
		{
			base.Dispose();

			ObjectManager.Remove(this.Id);
		}
	}
}