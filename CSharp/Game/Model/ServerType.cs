using System;

namespace Model
{
	[Flags]
	public enum ServerType
	{
		None = 0,
		Realm = 1,
		Gate = 2,
		City = 4,
		All = int.MaxValue,
	}
}