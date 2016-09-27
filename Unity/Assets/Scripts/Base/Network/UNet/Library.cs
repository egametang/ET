namespace Base
{
	internal static class Library
	{
		public static void Initialize()
		{
			int ret = NativeMethods.ENetInitialize();
			if (ret < 0)
			{
				throw new GameException($"Initialization failed, ret: {ret}");
			}
		}

		public static void Deinitialize()
		{
			NativeMethods.ENetDeinitialize();
		}

		public static uint Time
		{
			get
			{
				return NativeMethods.ENetTimeGet();
			}
			set
			{
				NativeMethods.ENetTimeSet(value);
			}
		}
	}
}