using UnityEngine;

namespace ET.Client
{
	[ComponentOf(typeof(Room))]
	public class LSCameraComponent : Entity, IAwake, ILateUpdate
	{
		// 战斗摄像机
		private Camera camera;

		public Transform Transform;

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

		private EntityRef<LSUnitView> unitView;

		public LSUnitView MyUnitView
		{
			get
			{
				return this.unitView;
			}
			set
			{
				this.unitView = value;
			}
		}

		public int index;
	}
}
