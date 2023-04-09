using UnityEngine;

namespace ET.Client
{
	public class CameraComponent : Entity, IAwake, ILateUpdate
	{
		// 战斗摄像机
		public Camera mainCamera;

		private long unitInstanceId;

		public Unit Unit
		{
			get
			{
				return Root.Instance.Get(this.unitInstanceId) as Unit;
			}
			set
			{
				this.unitInstanceId = value.InstanceId;
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
