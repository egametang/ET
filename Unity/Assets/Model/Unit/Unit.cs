using PF;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace ET
{
	public sealed class Unit: Entity
	{
		public int ConfigId;

		public UnitConfig Config
		{
			get
			{
				return UnitConfigCategory.Instance.Get(this.ConfigId);
			}
		}
		
		private Vector3 position;
		
		public Vector3 Position
		{
			get
			{
				return position;
			}
			set
			{
				this.position = value;
			}
		}

		private Quaternion rotation;

		public Quaternion Rotation
		{
			get
			{
				return rotation;
			}
			set
			{
				this.rotation = value;
			}
		}
	}
}