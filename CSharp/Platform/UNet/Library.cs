namespace UNet
{
	internal static class Library
	{
		public static void Initialize()
		{
			int ret = NativeMethods.EnetInitialize();
			if (ret < 0)
			{
				throw new UException(string.Format("Initialization failed, ret: {0}", ret));
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