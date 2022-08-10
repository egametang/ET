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
			self.mainCamera = Camera.main;
		}

		private static void LateUpdate(this CameraComponent self)
		{
			// 摄像机每帧更新位置
			self.UpdatePosition();
		}

		private static void UpdatePosition(this CameraComponent self)
		{
			Vector3 cameraPos = self.mainCamera.transform.position;
			self.mainCamera.transform.position = new Vector3(self.Unit.Position.x, cameraPos.y, self.Unit.Position.z - 1);
		}
	}
}
