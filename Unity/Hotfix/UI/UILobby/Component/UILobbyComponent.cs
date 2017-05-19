using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Model
{
	[EntityEvent(EntityEventId.UILobbyComponent)]
	public class UILobbyComponent: Component
	{
		public void Awake()
		{
			ReferenceCollector rc = this.GetOwner<UI>().GameObject.GetComponent<ReferenceCollector>();
			GameObject createRoom = rc.Get<GameObject>("CreateRoom");
			GameObject joinRoom = rc.Get<GameObject>("JoinRoom");
			createRoom.GetComponent<Button>().onClick.Add(() => this.OnCreateRoom());
			joinRoom.GetComponent<Button>().onClick.Add(() => this.OnJoinRoom());
		}

		private async void OnCreateRoom()
		{
			try
			{
				using (Session session = Game.Scene.GetComponent<NetOuterComponent>().Create("127.0.0.1:10001"))
				{
					R2C_Login r2CLogin = await session.Call<C2R_Login, R2C_Login>(new C2R_Login() { Account = "abcdef", Password = "111111" });
					Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(r2CLogin.Address);
					G2C_LoginGate g2CLoginGate = await gateSession.Call<C2G_LoginGate, G2C_LoginGate>(new C2G_LoginGate(r2CLogin.Key));

					Log.Info("登陆gate成功!");
				}
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}

		private void OnJoinRoom()
		{

		}
	}
}
