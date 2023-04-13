using UnityEngine;

namespace ET.Client
{
	[ComponentOf(typeof(Scene))]
	public class CameraComponent : Entity, IAwake, ILateUpdate
	{
		// 战斗摄像机
		private Camera camera;

		public Transform Transform;

		public long MyId
		{
			get;
			set;
		}

		public Camera Camera
		{
			get
			{
				return this.camera;
			}
			set
			{
				this.camera = value;
				this.Transform = this.camera.transform;
			}
		}
	}
}
