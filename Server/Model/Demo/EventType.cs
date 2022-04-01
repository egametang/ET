using UnityEngine;

namespace ET
{
	namespace EventType
	{
		public class AppStart
		{
		}
		
		public class ChangePosition
		{
			public Unit Unit;
			public Vector3 OldPos;
		}

		public class ChangeRotation
		{
			public Unit Unit;
		}

		public class MoveStart
		{
			public Unit Unit;
		}

		public class MoveStop
		{
			public Unit Unit;
		}

		public class UnitEnterSightRange
		{
			public AOIEntity A;
			public AOIEntity B;
		}

		public class UnitLeaveSightRange
		{
			public AOIEntity A;
			public AOIEntity B;
		}
	}
}