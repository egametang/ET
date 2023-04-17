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
			UnitF unit = self.GetMyUnit();
			if (unit != null)
			{
				Vector3 pos = unit.Position.ToVector();
				Vector3 cameraPos = self.Transform.position;
				self.Transform.position = new Vector3(pos.x, cameraPos.y, pos.z - 10);
			}
		}
		
		private static UnitF GetMyUnit(this CameraComponent self)
		{
			return self.GetParent<BattleScene>().GetMyUnitF();
		}
	}
}
