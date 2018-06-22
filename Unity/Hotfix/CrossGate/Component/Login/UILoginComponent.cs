using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace ETHotfix
{
    [ObjectSystem]
    public class UILoginComponentSystem : AwakeSystem<UILoginComponent, string>
    {
        public override void Awake(UILoginComponent self, string type)
        {
            self.MyType = type;
            self.Awake();
        }
    }

    public class UILoginComponent : UIBaseComponent
    {
        //区分倒计时自动销毁标记
        public string MyType;

        private InputField login_AccountInput;
        private InputField login_PasswordInput;
        private Button login_ServerchooeseBtn;
        private Button login_RegisterBtn;
        private Button login_LoginBtn;
        private GameObject LoginBg;
        private GameObject UILogin;
        private bool IsLogined;

        public void Awake()
        {
            this.Type = this.MyType;
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            login_AccountInput = rc.Get<GameObject>("Account").GetComponent<InputField>();
            login_PasswordInput = rc.Get<GameObject>("Password").GetComponent<InputField>();
            login_ServerchooeseBtn = rc.Get<GameObject>("Server_Btn").GetComponent<Button>();
            login_RegisterBtn = rc.Get<GameObject>("RegisterBtn").GetComponent<Button>();
            login_LoginBtn = rc.Get<GameObject>("LoginBtn_Btn").GetComponent<Button>();
            LoginBg = rc.Get<GameObject>("LoginBg");
            //UILogin = rc.Get<GameObject>("UILogin");

            login_ServerchooeseBtn.onClick.Add(Login_OnServerClick);
            login_RegisterBtn.onClick.Add(Login_OnRegisterClick);
            login_LoginBtn.onClick.Add(Login_OnLoginClick);

            //读取本地Playerfaps
            login_AccountInput.text = PlayerPrefs.GetString("Account");
            login_PasswordInput.text = PlayerPrefs.GetString("Password");
        }

        public override void Show()
        {
            LoginBg.transform.localScale = Vector3.zero;
            base.Show();
            LoginBg.transform.DOScale(Vector3.one, 0.1f);
        }

        public override void Close()
        {
            LoginBg.transform.DOScale(Vector3.zero, 0.1f).onComplete += () =>
            {
                base.Close();
            };
        }

        //选择线路
        private async void Login_OnServerClick()
        {
            //todo
        }

        //切换到注册界面
        private void Login_OnRegisterClick()
        {
            //todo音效
            //创造UI界面
            Game.Scene.GetComponent<UIComponent>().Close(UIType.UILogin);
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UIRegister);
        }

        //登录帐号
        private async void Login_OnLoginClick()
        {
            if (this.IsDisposed) return;

            //信息校验
            if (!GameTool.CharacterDetection(login_AccountInput.text) || !GameTool.CharacterDetection(login_PasswordInput.text))
            {
                GameTool.ShowPopMessage("帐号密码信息存在非法字符!");
                return;
            }

            //把帐号密码保存到本地Playerfaps
            PlayerPrefs.SetString("Account", login_AccountInput.text);
            PlayerPrefs.SetString("Password", login_PasswordInput.text);

            //打开加载界面
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UIWaitting);
            
            Session sessionWrap = null;
            try
            {
                //创建验证服务器连接
                IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);
                //创建一个ETModel层的Session
                ETModel.Session session = Game.Scene.ModelScene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
                //创建一个ETHotfix层的Session, ETHotfix的Session会通过ETModel层的Session发送消息
                sessionWrap = ComponentFactory.Create<Session, ETModel.Session>(session);

                //注册和sessionWrap短连接断开后的回调事件
                sessionWrap.session.GetComponent<SessionCallbackComponent>().DisposeCallback += s =>
                {
                    if (!IsLogined && (Game.Scene.GetComponent<SessionComponent>() == null || Game.Scene?.GetComponent<SessionComponent>()?.Session == null || Game.Scene.GetComponent<SessionComponent>().Session.IsDisposed))
                    {
                        Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);
                        GameTool.ShowPopMessage("连接失败!");
                    }
                };

                IsLogined = true;

                //验证帐号密码
                R2C_Login_Response r2C_Login_Response = await sessionWrap.Call(new C2R_Login_Request() { Account = login_AccountInput.text, Password = GameTool.GetMd5(login_PasswordInput.text) }) as R2C_Login_Response;

                //返回结果后断开Realm的连接
                sessionWrap.Dispose();

                //关闭Loading界面
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);
                if (this.IsDisposed) return;

                if (r2C_Login_Response?.Error != ErrorCode.ERR_Success)
                {
                    GameTool.ShowPopMessage("登录失败, 帐号或密码错误!");
                    return;
                }

                //创建Gate服务器连接
                connetEndPoint = NetworkHelper.ToIPEndPoint(r2C_Login_Response.Address);
                ETModel.Session gateSession = Game.Scene.ModelScene.GetComponent<NetOuterComponent>().Create(connetEndPoint);

                //创建一个ETHotfix层的Session, 并且保存到ETHotfix.SessionComponent中
                Game.Scene.AddComponent<SessionComponent>().Session = ComponentFactory.Create<Session, ETModel.Session>(gateSession);

                //添加连接断开组件，用于处理客户端连接断开, 当Session
                Game.Scene.GetComponent<SessionComponent>().Session.AddComponent<SessionOfflineComponent>();

                //登录Gate服务器
                G2C_LoginGate_Response g2C_LoginGate_Response = await SessionComponent.Instance.Session.Call(new C2G_LoginGate_Request() { Key = r2C_Login_Response.Key }) as G2C_LoginGate_Response;
                if (g2C_LoginGate_Response.Error == ErrorCode.ERR_ConnectGateKeyError)
                {
                    ErrorHelper.ShowErrorMessage(g2C_LoginGate_Response.Error);
                    Game.Scene.GetComponent<SessionComponent>().Session.Dispose();
                    return;
                }

                //将信息持有到本地
                ClientComponent.Instance.LocalUser = ComponentFactory.CreateWithId<User, long>(g2C_LoginGate_Response.PlayerID, g2C_LoginGate_Response.UserID);

                GameTool.ShowPopMessage("登录成功!");
                Game.Scene.GetComponent<UIComponent>().Remove(UIType.UILogin);

                if (g2C_LoginGate_Response.Info == null)
                {
                    //创建新角色:
                    Game.Scene.GetComponent<UIComponent>().Create(UIType.UICreateCharacter);
                }
                else
                {
                    //进入游戏:

                    //将信息持有到本地
                    ClientComponent.Instance.LocalRole = ComponentFactory.Create<Role>();
                    ClientComponent.Instance.LocalRole.BasicInfo = g2C_LoginGate_Response.Info;

                    //切换动画
                    Game.Scene.GetComponent<UIComponent>().Create(UIType.UILoadingScene);
                    Game.Scene.GetComponent<UIComponent>().Close(UIType.UIStartMenu);

                    //通过事件调用
                    Game.EventSystem.Run(EventIdType.LoginEnterMap);
                }
            }
            catch (Exception e)
            {
                //关闭Loading界面
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);
                GameTool.ShowPopMessage("登录失败, 未知异常!");
                Log.Error(e.ToStr());
            }
        }
    }
}
