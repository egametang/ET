using System;

namespace Model
{
	internal static class Library
	{
		public static void Initialize()
		{
			int ret = NativeMethods.enet_initialize();
			if (ret < 0)
			{
				throw new Exception($"Initialization failed, ret: {ret}");
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