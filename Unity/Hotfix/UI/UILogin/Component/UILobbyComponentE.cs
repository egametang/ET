using Base;
using UnityEngine;
using UnityEngine.UI;

namespace Model
{
	[EntityEvent(EntityEventId.UILobbyComponent)]
	public static class UILobbyComponentE
	{
		public static void Awake(this UILobbyComponent self)
		{
			ReferenceCollector rc = self.GetOwner<UI>().GameObject.GetComponent<ReferenceCollector>();
			GameObject createRoom = rc.Get<GameObject>("CreateRoom");
			GameObject joinRoom = rc.Get<GameObject>("JoinRoom");
			createRoom.GetComponent<Button>().onClick.Add(()=> self.OnCreateRoom());
			joinRoom.GetComponent<Button>().onClick.Add(()=>self.OnJoinRoom());
		}

		private static void OnCreateRoom(this UILobbyComponent self)
		{
			Log.Debug("create room");
		}

		private static void OnJoinRoom(this UILobbyComponent self)
		{

		}
	}
}
