namespace Base
{
	/// <summary>
	/// Component的Id与Owner Entity Id一样
	/// </summary>
	public abstract class Component: Object
	{
		public Entity Owner { get; set; }

		public T GetOwner<T>() where T: Entity
		{
			T owner = this.Owner as T;
			if (owner == null)
			{
				Log.Error($"Owner类型是{this.Owner.GetType().Name}, 无法转成: {typeof(T).Name}");
			}
			return owner;
		}

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
			base.Dispose();

			ObjectManager.Remove(this.Id);
		}
	}
}