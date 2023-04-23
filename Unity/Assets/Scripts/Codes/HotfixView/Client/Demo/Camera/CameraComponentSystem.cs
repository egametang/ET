using UnityEngine;

namespace ET.Client
{
	[FriendOf(typeof(CameraComponent))]
	public static class CameraComponentSystem
	{
		[ObjectSystem]
		public class CameraComponentAwakeSystem : AwakeSystem<CameraComponent>
		{
			protected override void Awake(CameraComponent self)
			{
				self.Awake();
			}
		}

		[ObjectSystem]
		public class CameraComponentLateUpdateSystem : LateUpdateSystem<CameraComponent>
		{
			protected override void LateUpdate(CameraComponent self)
			{
				self.LateUpdate();
			}
		}

		private static void Awake(this CameraComponent self)
		{
			self.Camera = Camera.main;
		}

		private static void LateUpdate(this CameraComponent self)
		{
			// 摄像机每帧更新位置

			LSUnitView lsUnit = self.MyUnitView;
			if (lsUnit == null)
			{
				self.MyUnitView = self.GetParent<Room>().GetComponent<LSUnitViewComponent>().GetMyLsUnitView();
			}

			if (lsUnit == null)
			{
				return;
			}

			Vector3 pos = lsUnit.Transform.position;
			Vector3 cameraPos = self.Transform.position;
			self.Transform.position = new Vector3(pos.x, cameraPos.y, pos.z - 10);
		}
	}
}
