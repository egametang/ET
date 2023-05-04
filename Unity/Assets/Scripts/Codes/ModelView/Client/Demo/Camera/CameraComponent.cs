using UnityEngine;

namespace ET.Client
{
	public class CameraComponent : Entity, IAwake, ILateUpdate
	{
		// 战斗摄像机
		public Camera mainCamera;

		private EntityRef<Unit> unit;

		public Unit Unit
		{
			get
			{
				return this.unit;
			}
			set
			{
				this.unit = value;
			}
		}

		public Camera MainCamera
		{
			get
			{
				return this.mainCamera;
			}
		}
	}
}
