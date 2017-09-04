using System;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Hotfix
{
	[ObjectEvent]
	public class UILoginComponentEvent : ObjectEvent<UILoginComponent>, IAwake
	{
		public void Awake()
		{
			this.Get().Awake();
		}
	}
	
	public class UILoginComponent: Component
	{
		private GameObject account;
		private GameObject loginBtn;

		public void Awake()
		{
			ReferenceCollector rc = this.GetOwner<UI>().GameObject.GetComponent<ReferenceCollector>();
			loginBtn = rc.Get<GameObject>("LoginBtn");
			loginBtn.GetComponent<Button>().onClick.Add(OnLogin);

			this.account = rc.Get<GameObject>("Account");
		}

		private async void OnLogin()
		{
			Session session = null;
			try
			{
				session = Hotfix.Scene.ModelScene.GetComponent<NetOuterComponent>().Create("127.0.0.1:10002");
				string account = this.account.GetComponent<InputField>().text;
				R2C_Login r2CLogin = await session.Call<R2C_Login>(new C2R_Login() { Account = account, Password = "111111" });
				Session gateSession = Hotfix.Scene.ModelScene.GetComponent<NetOuterComponent>().Create(r2CLogin.Address);
				G2C_LoginGate g2CLoginGate = await gateSession.Call<G2C_LoginGate>(new C2G_LoginGate(r2CLogin.Key));
				
				//Log.Debug($"{JsonHelper.ToJson(g2CLoginGate)}");

				Log.Info("登陆gate成功!");

				// 发送一个actor消息
				gateSession.Send(new Actor_Test() { Info = "message client->gate->map->gate->client" });

				// 向actor发起一次rpc调用
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
	}
}
