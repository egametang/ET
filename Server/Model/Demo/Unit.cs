using UnityEngine;

namespace ET
{
	public enum UnitType
	{
		Hero,
		Npc
	}

	public class UnitAwakeSystem : AwakeSystem<Unit, UnitType>
	{
		public override void Awake(Unit self, UnitType a)
		{
			self.Awake(a);
		}
	}

	public sealed class Unit: Entity
	{
		public UnitType UnitType { get; private set; }
		
		public Vector3 Position { get; set; }
		
		public void Awake(UnitType unitType)
		{
			this.UnitType = unitType;
		}

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}