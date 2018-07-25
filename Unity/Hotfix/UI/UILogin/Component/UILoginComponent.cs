using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
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

				// 创建一个ETModel层的Session
				ETModel.Session session = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
				// 创建一个ETHotfix层的Session, ETHotfix的Session会通过ETModel层的Session发送消息
				Session realmSession = ComponentFactory.Create<Session, ETModel.Session>(session);
				R2C_Login r2CLogin = (R2C_Login) await realmSession.Call(new C2R_Login() { Account = text, Password = "111111" });
				realmSession.Dispose();

				connetEndPoint = NetworkHelper.ToIPEndPoint(r2CLogin.Address);
				// 创建一个ETModel层的Session,并且保存到ETModel.SessionComponent中
				ETModel.Session gateSession = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
				ETModel.Game.Scene.AddComponent<ETModel.SessionComponent>().Session = gateSession;
				
				// 创建一个ETHotfix层的Session, 并且保存到ETHotfix.SessionComponent中
				Game.Scene.AddComponent<SessionComponent>().Session = ComponentFactory.Create<Session, ETModel.Session>(gateSession);
				
				G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2CLogin.Key });

				Log.Info("登陆gate成功!");

				// 创建Player
				Player player = ETModel.ComponentFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
				PlayerComponent playerComponent = ETModel.Game.Scene.GetComponent<PlayerComponent>();
				playerComponent.MyPlayer = player;

				Game.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
				Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}
	}
}
