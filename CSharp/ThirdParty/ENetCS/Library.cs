namespace ENet
{
	public static class Library
	{
		public static void Initialize()
		{
			var inits = new ENetCallbacks();
			int ret = Native.enet_initialize_with_callbacks(Native.ENET_VERSION, ref inits);
			if (ret < 0)
			{
				throw new ENetException(ret, "Initialization failed.");
			}
		}

		public static void Deinitialize()
		{
			Native.enet_deinitialize();
		}

		public static uint Time
		{
			get
			{
				return Native.enet_time_get();
			}
			set
			{
				Native.enet_time_set(value);
			}
		}
	}
}