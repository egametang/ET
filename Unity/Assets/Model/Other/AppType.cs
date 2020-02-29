using System;
using System.Collections.Generic;

namespace ET
{
	[Flags]
	public enum AppType
	{
		Manager = 1,
		Realm = 2,
		Gate = 3,
		Http = 4,
		DB = 5,
		Location = 6,
		Map = 7,

		Robot = 20,
		Benchmark = 21,
	}
}