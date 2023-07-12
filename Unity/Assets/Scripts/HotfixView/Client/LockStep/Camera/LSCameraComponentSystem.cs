using UnityEngine;

namespace ET.Client
{
	[EntitySystemOf(typeof(LSCameraComponent))]
	[FriendOf(typeof(LSCameraComponent))]
	public static partial class LSCameraComponentSystem
	{
		[EntitySystem]
		private static void Awake(this LSCameraComponent self)
		{
			self.Camera = Camera.main;
			self.Camera.transform.rotation = Quaternion.Euler(new Vector3(20, 0, 0));
		}
		
		[EntitySystem]
		private static void LateUpdate(this LSCameraComponent self)
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
