namespace ENet
{
	internal static class Library
	{
		public static void Initialize()
		{
			var inits = new ENetCallbacks();
			int ret = NativeMethods.EnetInitializeWithCallbacks(
				NativeMethods.ENET_VERSION, ref inits);
			if (ret < 0)
			{
				throw new EException(string.Format("Initialization failed, ret: {0}", ret));
			}
		}

		public static void Deinitialize()
		{
			NativeMethods.EnetDeinitialize();
		}

		public static uint Time
		{
			get
			{
				return NativeMethods.EnetTimeGet();
			}
			set
			{
				NativeMethods.EnetTimeSet(value);
			}
		}
	}
}