
namespace UniFramework.Singleton
{
	public abstract class SingletonInstance<T> where T : class, ISingleton
	{
		private static T _instance;
		public static T Instance
		{
			get
			{
				if (_instance == null)
					UniLogger.Error($"{typeof(T)} is not create. Use {nameof(UniSingleton)}.{nameof(UniSingleton.CreateSingleton)} create.");
				return _instance;
			}
		}

		protected SingletonInstance()
		{
			if (_instance != null)
				throw new System.Exception($"{typeof(T)} instance already created.");
			_instance = this as T;
		}
		protected void DestroyInstance()
		{
			_instance = null;
		}
	}
}