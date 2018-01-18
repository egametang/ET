using System;
using System.Net;
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
				Session session = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
				string text = this.account.GetComponent<InputField>().text;


				R2C_Login r2CLogin = (R2C_Login)await session.Call(new C2R_Login() { Account = text, Password = "111111" }, true);
				if (r2CLogin.Error != ErrorCode.ERR_Success)
				{
					Log.Error(r2CLogin.Error.ToString());
					return;
				}

				connetEndPoint = NetworkHelper.ToIPEndPoint(r2CLogin.Address);
				Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
				Game.Scene.AddComponent<SessionComponent>().Session = gateSession;

				G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2CLogin.Key }, true);

				Log.Info("登陆gate成功!");

				// 创建Player
				Player player = Model.EntityFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
				PlayerComponent playerComponent = Game.Scene.GetComponent<PlayerComponent>();
				playerComponent.MyPlayer = player;

				Hotfix.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
				Hotfix.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			}
			catch (Exception e)
			{
				Log.Error(e.ToString());
			}
		}
	}
}
