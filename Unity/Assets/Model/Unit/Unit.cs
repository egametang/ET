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

	}
}