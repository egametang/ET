using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
	/// <summary>
	/// Component的Id与Owner Entity Id一样
	/// </summary>
	public abstract class Component<T>: Object where T : Entity<T>
	{
		private T owner;

		[BsonIgnore]
		public T Owner
		{
			get
			{
				return this.owner;
			}
			set
			{
				this.owner = value;
			}
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