using System;

namespace Base
{
	internal static class Library
	{
		public static void Initialize()
		{
			int ret = NativeMethods.ENetInitialize();
			if (ret < 0)
			{
				throw new Exception($"Initialization failed, ret: {ret}");
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