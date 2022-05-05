using UnityEngine;

namespace ET.Client
{
	public class CameraComponent : Entity, IAwake, ILateUpdate
	{
		// 战斗摄像机
		public Camera mainCamera;

		public Unit Unit;

		public Camera MainCamera
		{
			get
			{
				return this.mainCamera;
			}
		}
	}
}
