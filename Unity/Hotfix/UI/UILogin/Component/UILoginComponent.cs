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

		private void OnLogin()
		{
			Session session = null;
			IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);
			session = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
			string text = this.account.GetComponent<InputField>().text;
			session.CallWithAction(new C2R_Login() { Account = text, Password = "111111" }, (response) => LoginOK(session, response));
		}

		private void LoginOK(Session loginSession, AResponse response)
		{
			loginSession.Dispose();

			R2C_Login r2CLogin = (R2C_Login) response;
			if (r2CLogin.Error != ErrorCode.ERR_Success)
			{
				Log.Error(r2CLogin.Error.ToString());
				return;
			}

			IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(r2CLogin.Address);
			Session gateSession = Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
			Game.Scene.AddComponent<SessionComponent>().Session = gateSession;

			SessionComponent.Instance.Session.CallWithAction(new C2G_LoginGate() { Key = r2CLogin.Key },
				(response2)=>LoginGateOk(response2)
			);

		}

		private void LoginGateOk(AResponse response)
		{
			G2C_LoginGate g2CLoginGate = (G2C_LoginGate) response;
			if (g2CLoginGate.Error != ErrorCode.ERR_Success)
			{
				Log.Error(g2CLoginGate.Error.ToString());
				return;
			}

			Log.Info("登陆gate成功!");

			// 创建Player
			Player player = Model.EntityFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
			PlayerComponent playerComponent = Game.Scene.GetComponent<PlayerComponent>();
			playerComponent.MyPlayer = player;

			Hotfix.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
			Hotfix.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
		}
	}
}
