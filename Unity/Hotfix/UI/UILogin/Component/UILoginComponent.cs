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
			ReferenceCollector rc = this.GetEntity<UI>().GameObject.GetComponent<ReferenceCollector>();
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
				string text = this.account.GetComponent<InputField>().text;
				R2C_Login r2CLogin = await session.Call<R2C_Login>(new C2R_Login() { Account = text, Password = "111111" });
				Session gateSession = Hotfix.Scene.ModelScene.GetComponent<NetOuterComponent>().Create(r2CLogin.Address);
				Game.Scene.AddComponent<SessionComponent>().Session = gateSession;

				G2C_LoginGate g2CLoginGate = await SessionComponent.Instance.Session.Call<G2C_LoginGate>(new C2G_LoginGate(r2CLogin.Key));
				Log.Info("登陆gate成功!");

				Hotfix.Scene.GetComponent<UIComponent>().Create(UIType.Lobby);
				Hotfix.Scene.GetComponent<UIComponent>().Remove(UIType.Login);
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
