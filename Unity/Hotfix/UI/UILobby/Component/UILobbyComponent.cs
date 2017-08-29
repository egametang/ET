using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Hotfix
{
	[ObjectEvent(EntityEventId.UILobbyComponent)]
	public class UILobbyComponent: Component, IAwake
	{
		public void Awake()
		{
			ReferenceCollector rc = this.GetOwner<UI>().GameObject.GetComponent<ReferenceCollector>();
			GameObject createRoom = rc.Get<GameObject>("CreateRoom");
			GameObject joinRoom = rc.Get<GameObject>("JoinRoom");
			createRoom.GetComponent<Button>().onClick.Add(OnCreateRoom);
			joinRoom.GetComponent<Button>().onClick.Add(OnJoinRoom);
		}

		private static async void OnCreateRoom()
		{
			Session session = null;
			try
			{
				session = Hotfix.Scene.ModelScene.GetComponent<NetOuterComponent>().Create("127.0.0.1:10001");
				R2C_Login r2CLogin = await session.Call<R2C_Login>(new C2R_Login() { Account = "abcdef", Password = "111111" });
				Session gateSession = Hotfix.Scene.ModelScene.GetComponent<NetOuterComponent>().Create(r2CLogin.Address);
				G2C_LoginGate g2CLoginGate = await gateSession.Call<G2C_LoginGate>(new C2G_LoginGate(r2CLogin.Key));
				
				Log.Debug($"{JsonHelper.ToJson(g2CLoginGate)}");

				Log.Info("登陆gate成功!");

				// 发送一个actor消息
				//gateSession.Send(new Actor_Test() { Info = "message client->gate->map->gate->client" });

				ActorRpc_TestResponse response = await gateSession.Call<ActorRpc_TestResponse>(new ActorRpc_TestRequest() { request = "request actor test rpc" });
				Log.Info($"recv response: {JsonHelper.ToJson(response)}");
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
			finally
			{
				session?.Dispose();
			}
		}

		private static void OnJoinRoom()
		{

		}
	}
}
