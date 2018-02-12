using System;
using System.Net;
using Model;
using UnityEngine;
using UnityEngine.UI;

namespace Hotfix
{
	[ObjectSystem]
	public class UiLoginComponentSystem : AwakeSystem<UILoginComponent>
	{
		public override void Awake(UILoginComponent self)
		{
			self.Awake();
		}
	}
	
	public class UILoginComponent: Component
	{
		private GameObject account;
		private GameObject loginBtn;

		public void Awake()
		{
			ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			loginBtn = rc.Get<GameObject>("LoginBtn");
			loginBtn.GetComponent<Button>().onClick.Add(OnLogin);
			this.account = rc.Get<GameObject>("Account");
		}

		public async void OnLogin()
		{
			try
			{
				IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);

				string text = this.account.GetComponent<InputField>().text;

				R2C_Login r2CLogin;
				using (Session session = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint))
				{
					r2CLogin = (R2C_Login) await session.Call(new C2R_Login() { Account = text, Password = "111111" });
				}

				connetEndPoint = NetworkHelper.ToIPEndPoint(r2CLogin.Address);
				Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
				Game.Scene.AddComponent<SessionComponent>().Session = gateSession;
				G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2CLogin.Key });

				Log.Info("登陆gate成功!");

				// 创建Player
				Player player = Model.ComponentFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
				PlayerComponent playerComponent = Game.Scene.GetComponent<PlayerComponent>();
				playerComponent.MyPlayer = player;

				Hotfix.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
				Hotfix.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			}
			catch (Exception e)
			{
				Log.Error(e.ToStr());
			}
		}
	}
}
