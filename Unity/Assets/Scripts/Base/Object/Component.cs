namespace Base
{
	/// <summary>
	/// Component的Id与Owner Entity Id一样
	/// </summary>
	public abstract class Component: Object
	{
		public Unit Owner { get; set; }

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