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

    public class UILoginComponent : Component
    {
        private GameObject Login;
        private InputField account;
        private GameObject loginBtn;
        private InputField Password;

        private GameObject Regist;
        private InputField Password_Ag;
        private Button RegistBtn;

        private Button Btn_GoToRegist;
        private Button Btn_GoToLogin;


        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            Login = rc.Get<GameObject>("Login");
            Regist = rc.Get<GameObject>("Regist");
            loginBtn = rc.Get<GameObject>("LoginBtn");
            loginBtn.GetComponent<Button>().onClick.Add(OnLogin);
            this.account = rc.Get<GameObject>("Account").GetComponent<InputField>();
            Password = rc.Get<GameObject>("Password").GetComponent<InputField>();
            Btn_GoToRegist = rc.Get<GameObject>("Btn_GoToRegist").GetComponent<Button>();
            Btn_GoToLogin = rc.Get<GameObject>("Btn_GoToLogin").GetComponent<Button>();
            RegistBtn = rc.Get<GameObject>("RegistBtn").GetComponent<Button>();
            Btn_GoToRegist.onClick.Add(BtnClick_GotoRegist);
            Btn_GoToLogin.onClick.Add(BtnClick_GotoLogin);
            RegistBtn.onClick.Add(Btn_RegistAccount);
            Password_Ag = rc.Get<GameObject>("Password_Ag").GetComponent<InputField>();

        }

        public async void OnLogin()
        {
            SessionWrap sessionWrap = null;
            try
            {
                //IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);

                //string text = this.account.text;

                //Session session = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
                //sessionWrap = new SessionWrap(session);
                //R2C_Login r2CLogin = (R2C_Login)await sessionWrap.Call(new C2R_Login() { Account = text, Password = "111111" });
                //sessionWrap.Dispose();

                //connetEndPoint = NetworkHelper.ToIPEndPoint(r2CLogin.Address);
                //Session gateSession = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
                //Game.Scene.AddComponent<SessionWrapComponent>().Session = new SessionWrap(gateSession);
                //ETModel.Game.Scene.AddComponent<SessionComponent>().Session = gateSession;
                //G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await SessionWrapComponent.Instance.Session.Call(new C2G_LoginGate() { Key = r2CLogin.Key });

                //Log.Info("登陆gate成功!");

                //// 创建Player
                //Player player = ETModel.ComponentFactory.CreateWithId<Player>(g2CLoginGate.PlayerId);
                //PlayerComponent playerComponent = ETModel.Game.Scene.GetComponent<PlayerComponent>();
                //playerComponent.MyPlayer = player;

                //Game.Scene.GetComponent<UIComponent>().Create(UIType.UILobby);
                Game.Scene.GetComponent<UIComponent>().Create(UIType.HG_UIMenu);
                Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);
            }
            catch (Exception e)
            {
                sessionWrap?.Dispose();
                Log.Error(e);
            }
        }

        public async void BtnClick_GotoRegist()
        {
            Regist.gameObject.SetActive(true);
            Login.gameObject.SetActive(false);
        }
        public async void BtnClick_GotoLogin()
        {
            Regist.gameObject.SetActive(false);
            Login.gameObject.SetActive(true);
        }
        public async void Btn_RegistAccount()
        {
            SessionWrap sessionWrap = null;
            try
            {
                IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);

                string text = this.account.GetComponent<InputField>().text;

                Session session = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(connetEndPoint);

                sessionWrap = new SessionWrap(session);
                R2C_Register_Ack r2CLogin = (R2C_Register_Ack)await sessionWrap.Call(new C2R_Register_Req() { Account = text, Password = "111111" });
                sessionWrap.Dispose();

                if (r2CLogin.Error == ErrorCode.ERR_AccountOrPasswordError)
                {
                    account.text = "";
                    Password.text = "";
                    return;
                }
                OnLogin();
            }
            catch (Exception e)
            {
                sessionWrap?.Dispose();
                Log.Error(e);
            }
        }
    }
}
