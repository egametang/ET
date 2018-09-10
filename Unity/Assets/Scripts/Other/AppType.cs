using System;
using System.Collections.Generic;

namespace ETModel
{
	[Flags]
	public enum AppType
	{
		None = 0,
		Manager = 1,
		Realm = 1 << 1,
		Gate = 1 << 2,
		Http = 1 << 3,
		DB = 1 << 4,
		Location = 1 << 5,
		Map = 1 << 6,

		BenchmarkWebsocketServer = 1 << 27,
		BenchmarkWebsocketClient = 1 << 28,
		Robot = 1 << 29,
		Benchmark = 1 << 30,
		Client = 1 << 31,

		// 7
		AllServer = Manager | Realm | Gate | Http | DB | Location | Map | BenchmarkWebsocketServer
	}

	public static class AppTypeHelper
	{
		public static List<AppType> GetServerTypes()
		{
			List<AppType> appTypes = new List<AppType> { AppType.Manager, AppType.Realm, AppType.Gate };
			return appTypes;
		}

		public static bool Is(this AppType a, AppType b)
		{
			if ((a & b) != 0)
			{
				return true;
			}
			return false;
		}
	}
}