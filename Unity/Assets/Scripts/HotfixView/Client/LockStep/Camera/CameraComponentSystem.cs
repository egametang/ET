using UnityEngine;

namespace ET.Client
{
	[FriendOf(typeof(CameraComponent))]
	public static class CameraComponentSystem
	{
		[ObjectSystem]
		public class AwakeSystem : AwakeSystem<CameraComponent>
		{
			protected override void Awake(CameraComponent self)
			{
				self.Awake();
			}
		}

		[ObjectSystem]
		public class LateUpdateSystem : LateUpdateSystem<CameraComponent>
		{
			protected override void LateUpdate(CameraComponent self)
			{
				self.LateUpdate();
			}
		}

		private static void Awake(this CameraComponent self)
		{
			self.Camera = Camera.main;
			self.Camera.transform.rotation = Quaternion.Euler(new Vector3(20, 0, 0));
		}

		private static void LateUpdate(this CameraComponent self)
		{
			// 摄像机每帧更新位置
			Room room = self.GetParent<Room>();
			if (room.IsReplay)
			{
				if (Input.GetKeyDown(KeyCode.Tab))
				{
					++self.index;
					self.MyUnitView = new LSUnitView();
				}
			}

			LSUnitView lsUnit = self.MyUnitView;
			if (lsUnit == null)
			{
				long id = room.IsReplay? room.PlayerIds[self.index % room.PlayerIds.Count] : room.GetParent<Scene>().GetComponent<PlayerComponent>().MyId;
				self.MyUnitView = room.GetComponent<LSUnitViewComponent>().GetChild<LSUnitView>(id);
			}

			if (lsUnit == null)
			{
				return;
			}

			Vector3 pos = lsUnit.Transform.position;
			self.Transform.position = new Vector3(pos.x, pos.y + 3, pos.z - 5);
		}
	}
}
