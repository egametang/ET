using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace ET
{
	public sealed class Unit: Entity
	{
		// 先放这里，去掉ViewGO，后面挪到显示层
		public GameObject GameObject;
		
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