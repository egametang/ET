using PF;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace ETModel
{
	public enum UnitType
	{
		Hero,
		Npc
	}
	
	public sealed class Unit: Entity
	{
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
			if (this.IsDisposed)
			{
				return;
			}

			base.Dispose();
		}
	}
}