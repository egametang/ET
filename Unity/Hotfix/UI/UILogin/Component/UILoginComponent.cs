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
		private InputField account;
        private InputField password;
		private GameObject loginBtn;

		public void Awake()
		{
			ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
			loginBtn = rc.Get<GameObject>("LoginBtn");
			loginBtn.GetComponent<Button>().onClick.Add(OnLogin);
			account = rc.Get<GameObject>("Account").GetComponent<InputField>();
            password = rc.Get<GameObject>("Password").GetComponent<InputField>();
		}

		public async void OnLogin()
		{
			SessionWrap sessionWrap = null;
			try
			{
                //从全局配置文件中，获取服务器地址
				IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);

				Session session = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
				sessionWrap = new SessionWrap(session);
				R2C_Login r2CLogin = (R2C_Login) await sessionWrap.Call(new C2R_Login() { Account = account.text, Password = password.text });

                if(r2CLogin.Error == ErrorCode.ERR_AccountOrPasswordError)
                {
                    Log.Error("账号或密码错误");
                    return;
                }
                else if(r2CLogin.Error == ErrorCode.ERR_AccountOrPasswordUnder6)
                {
                    Log.Error("账号密码不能少于6位数");
                    return;
                }

                sessionWrap.Dispose();
				connetEndPoint = NetworkHelper.ToIPEndPoint(r2CLogin.Address);
				Session gateSession = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
				Game.Scene.AddComponent<SessionWrapComponent>().Session = new SessionWrap(gateSession);

                //保存连接,之后所有请求将通过这个连接发送
                ETModel.Game.Scene.AddComponent<SessionComponent>().Session = gateSession;
				G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionWrapComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2CLogin.Key });

                if(g2CLoginGate.Error == ErrorCode.ERR_ConnectGateKeyError)
                {
                    Log.Error("连接网关服务器超时");
                    return;
                }

                Log.Info("登陆gate成功!");

                // 创建User
                User user = ETModel.ComponentFactory.CreateWithId<User>(g2CLoginGate.PlayerId);
                UserComponent userComponent = ETModel.Game.Scene.GetComponent<UserComponent>();
                userComponent.LocalPlayer = user;

                Game.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
				Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
			}
			catch (Exception e)
			{
				sessionWrap?.Dispose();
				Log.Error(e);
			}
		}
	}
}
