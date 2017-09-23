using UnityEngine;

namespace Model
{
	public enum UnitType
	{
		Hero,
		Npc
	}

	[ObjectEvent]
	public class UnitEvent : ObjectEvent<Unit>, IAwake<UnitType>
	{
		public void Awake(UnitType unitType)
		{
			this.Get().Awake(unitType);
		}
	}

	public sealed class Unit: Entity
	{
		public UnitType UnitType { get; private set; }

		public VInt3 IntPos;

		public GameObject GameObject;
		
		public void Awake(UnitType unitType)
		{
			this.UnitType = unitType;
		}

		public Vector3 Position
		{
			get
			{
				return GameObject.transform.position;
			}
			set
			{
				GameObject.transform.position = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return GameObject.transform.rotation;
			}
			set
			{
				GameObject.transform.rotation = value;
			}
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}