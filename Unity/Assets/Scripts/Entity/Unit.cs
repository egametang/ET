using UnityEngine;

namespace Model
{
	public enum UnitType
	{
		Hero,
		Npc
	}

	[ObjectSystem]
	public class UnitSystem : ObjectSystem<Unit>
	{
	}

	public sealed class Unit: Entity
	{
		public VInt3 IntPos;

		public GameObject GameObject;
		
		public void Awake()
		{
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