using System;

namespace Model
{
	[Flags]
	public enum EntityEventType
	{
		Awake = 1,
		Awake1 = 2,
		Awake2 = 4,
		Awake3 = 8,
		Update = 16,
		Load = 32,
		LateUpdate = 64
	}
}
