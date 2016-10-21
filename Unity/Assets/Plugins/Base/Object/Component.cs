using MongoDB.Bson.Serialization.Attributes;

namespace Base
{
	/// <summary>
	/// Component的Id与Owner Entity Id一样
	/// </summary>
	public abstract class Component : Object
	{
		[BsonIgnore]
		public Entity Owner { get; set; }

		protected Component()
		{
			ObjectManager.Add(this);
		}

		protected Component(long id): base(id)
		{
			ObjectManager.Add(this);
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

			ObjectManager.Remove(this.Id);
		}
	}
}