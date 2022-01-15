using UnityEngine;

namespace ET
{
	namespace EventType
	{
		public struct AppStart
		{
		}
		
		public struct ChangePosition
		{
			public Unit Unit;
			public Vector3 OldPos;
		}

		public struct ChangeRotation
		{
			public Unit Unit;
		}

		public struct MoveStart
		{
			public Unit Unit;
		}

		public struct MoveStop
		{
			public Unit Unit;
		}

		public struct UnitEnterSightRange
		{
			public AOIEntity A;
			public AOIEntity B;
		}

		public struct UnitLeaveSightRange
		{
			public AOIEntity A;
			public AOIEntity B;
		}
	}
}