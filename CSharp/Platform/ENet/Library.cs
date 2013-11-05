namespace ENet
{
	internal static class Library
	{
		public static void Initialize()
		{
			var inits = new ENetCallbacks();
			int ret = NativeMethods.enet_initialize_with_callbacks(
				NativeMethods.ENET_VERSION, ref inits);
			if (ret < 0)
			{
				throw new EException(string.Format("Initialization failed, ret: {0}", ret));
			}
		}

		public static void Deinitialize()
		{
			NativeMethods.enet_deinitialize();
		}

		public static uint Time
		{
			get
			{
				return NativeMethods.enet_time_get();
			}
			set
			{
				NativeMethods.enet_time_set(value);
			}
		}
	}
}