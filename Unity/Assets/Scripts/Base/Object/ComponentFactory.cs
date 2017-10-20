namespace Model
{
	public static class ComponentFactory
	{
		public static T Create<T>(Entity entity) where T : Component
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Entity = entity;
			ObjectEvents.Instance.Awake(disposer);
			return disposer;
		}

		public static T Create<T, A>(Entity entity, A a) where T : Component
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Entity = entity;
			ObjectEvents.Instance.Awake(disposer, a);
			return disposer;
		}

		public static T Create<T, A, B>(Entity entity, A a, B b) where T : Component
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Entity = entity;
			ObjectEvents.Instance.Awake(disposer, a, b);
			return disposer;
		}

		public static T Create<T, A, B, C>(Entity entity, A a, B b, C c) where T : Component
		{
			T disposer = ObjectPool.Instance.Fetch<T>();
			disposer.Entity = entity;
			ObjectEvents.Instance.Awake(disposer, a, b, c);
			return disposer;
		}
	}
}
