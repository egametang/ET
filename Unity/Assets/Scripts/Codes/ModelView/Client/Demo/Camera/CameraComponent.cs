using UnityEngine;

namespace ET.Client
{
	[ComponentOf(typeof(BattleScene))]
	public class CameraComponent : Entity, IAwake, ILateUpdate
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

		private long unitViewInstanceId;

		public LSUnitView MyUnitView
		{
			get
			{
				return Root.Instance.Get(this.unitViewInstanceId) as LSUnitView;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.unitViewInstanceId = value.InstanceId;
			}
		}
	}
}
