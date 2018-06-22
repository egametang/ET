using System;
using System.Net;
using DG.Tweening;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIRegisterComponentSystem : AwakeSystem<UIRegisterComponent, string>
    {
        public override void Awake(UIRegisterComponent self, string type)
        {
            self.MyType = type;
            self.Awake();
        }
    }

    public class UIRegisterComponent : UIBaseComponent
    {
        //区分倒计时自动销毁标记
        public string MyType;

        private InputField registerAccountInput;
        private InputField registerPasswordInput;
        private InputField registerRepasswordInput;
        private InputField safeQuestionInput;
        private InputField safeAnwserInput;
        private InputField safeReAnwserInput;
        private Button register_RegisterBtn;
        private Button register_BackBtn;
        private GameObject UIRegister;
        private GameObject UIRegisterBg;

        public void Awake()
        {
            this.Type = this.MyType;
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

            registerAccountInput = rc.Get<GameObject>("RegisterAccount").GetComponent<InputField>();
            registerPasswordInput = rc.Get<GameObject>("RegisterPassword").GetComponent<InputField>();
            registerRepasswordInput = rc.Get<GameObject>("RegisterRePassword").GetComponent<InputField>();
            safeQuestionInput = rc.Get<GameObject>("SafeQuestion").GetComponent<InputField>();
            safeAnwserInput = rc.Get<GameObject>("SafeAnswer").GetComponent<InputField>();
            safeReAnwserInput = rc.Get<GameObject>("ReSafeAnswer").GetComponent<InputField>();
            register_RegisterBtn = rc.Get<GameObject>("Register_Register_Btn").GetComponent<Button>();
            register_BackBtn = rc.Get<GameObject>("Register_Back_Btn").GetComponent<Button>();
            //UIRegister = rc.Get<GameObject>("UIRegister");
            UIRegisterBg = rc.Get<GameObject>("UIRegisterBg");

            register_RegisterBtn.onClick.Add(Register_OnRegisterClick);
            register_BackBtn.onClick.Add(Register_OnRegisterBackClick);
        }

        public override void Show()
        {
            UIRegisterBg.transform.localScale = Vector3.zero;
            base.Show();
            UIRegisterBg.transform.DOScale(Vector3.one, 0.1f);
        }

        public override void Close()
        {
            UIRegisterBg.transform.DOScale(Vector3.zero, 0.1f).onComplete += () =>
            {
                base.Close();
            };
        }

        //注册新帐号
        private async void Register_OnRegisterClick()
        {
            if (this.IsDisposed) return;

            //信息校验
            if (registerAccountInput.text.Length < 6)
            {
                GameTool.ShowPopMessage("帐号字符不能少于6位数!");
                return;
            }
            if (registerAccountInput.text.Length > 10)
            {
                GameTool.ShowPopMessage("帐号字符不能大于10位数!");
                return;
            }
            if (registerPasswordInput.text.Length < 6)
            {
                GameTool.ShowPopMessage("密码字符不能少于6位数!");
                return;
            }
            if (registerPasswordInput.text.Length > 10)
            {
                GameTool.ShowPopMessage("密码字符不能大于10位数!");
                return;
            }
            if (registerPasswordInput.text != registerRepasswordInput.text)
            {
                GameTool.ShowPopMessage("两次密码结果不一致!");
                return;
            }
            if (registerAccountInput.text == registerPasswordInput.text)
            {
                GameTool.ShowPopMessage("帐号密码信息不能相同!");
                return;
            }
            if (safeQuestionInput.text.Length < 6)
            {
                GameTool.ShowPopMessage("安全问题字符不能少于6位数!");
                return;
            }
            if (safeQuestionInput.text.Length > 12)
            {
                GameTool.ShowPopMessage("安全问题字符不能大于12位数!");
                return;
            }
            if (safeAnwserInput.text.Length < 6)
            {
                GameTool.ShowPopMessage("安全答案字符不能少于6位数!");
                return;
            }
            if (safeAnwserInput.text.Length > 12)
            {
                GameTool.ShowPopMessage("安全答案字符不能大于12位数!");
                return;
            }
            if (safeAnwserInput.text != safeReAnwserInput.text)
            {
                GameTool.ShowPopMessage("两次安全答案结果不一致!");
                return;
            }

            //非法字符检测
            if (!GameTool.CharacterDetection(registerAccountInput.text) || !GameTool.CharacterDetection(registerPasswordInput.text) ||
                !GameTool.CharacterDetection(registerRepasswordInput.text) || !GameTool.CharacterDetection(safeQuestionInput.text) ||
                !GameTool.CharacterDetection(safeAnwserInput.text) || !GameTool.CharacterDetection(safeReAnwserInput.text))
            {
                GameTool.ShowPopMessage("信息里存在非法字符!");
                return;
            }

            //开始注册:

            //显示Loading界面
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UIWaitting);
            Session sessionWrap = null;
            try
            {
                //创建验证服务器连接
                IPEndPoint connetEndPoint = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);
                ETModel.Session session = Game.Scene.ModelScene.GetComponent<NetOuterComponent>().Create(connetEndPoint);
                sessionWrap = ComponentFactory.Create<Session, ETModel.Session>(session);

                //注册和sessionWrap短连接断开后的回调事件
                sessionWrap.session.GetComponent<SessionCallbackComponent>().DisposeCallback += s =>
                {
                    if (Game.Scene.GetComponent<UIComponent>().Get(UIType.UIRegister) != null)
                    {
                        Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);
                        GameTool.ShowPopMessage("连接失败!");
                    }
                };

                //发送注册请求
                C2R_Register_Request c2R_Register_Req = new C2R_Register_Request
                {
                    Account = registerAccountInput.text,
                    Password = registerPasswordInput.text,
                    SafeQuestion = safeQuestionInput.text,
                    SafeAnswer = safeAnwserInput.text
                };

                //服务器返回注册信息
                R2C_Register_Response r2C_Register_Ack = await sessionWrap.Call(c2R_Register_Req) as R2C_Register_Response;

                //返回结果后断开Realm的连接
                sessionWrap.session.GetComponent<SessionCallbackComponent>().DisposeCallback = null;
                sessionWrap.Dispose();

                if (this.IsDisposed) return;

                //关闭Loading界面
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);

                if (r2C_Register_Ack.Error != ErrorCode.ERR_Success)
                {
                    ErrorHelper.ShowErrorMessage(r2C_Register_Ack.Error);
                    return;
                }

                //注册成功
                GameTool.ShowPopMessage("恭喜你, 帐号已注册成功!");

                //清空注册面板
                registerAccountInput.text = "";
                registerPasswordInput.text = "";
                registerRepasswordInput.text = "";
                safeQuestionInput.text = "";
                safeAnwserInput.text = "";
                safeReAnwserInput.text = "";

                //跳转回登录界面
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIRegister);
                Game.Scene.GetComponent<UIComponent>().Create(UIType.UILogin);
            }
            catch (Exception e)
            {
                //关闭Loading界面
                Game.Scene.GetComponent<UIComponent>().Close(UIType.UIWaitting);
                GameTool.ShowPopMessage("注册失败, 未知异常!");
                Log.Error(e.ToStr());
            }
        }

        //返回登录界面
        private void Register_OnRegisterBackClick()
        {
            //todo音效
            Game.Scene.GetComponent<UIComponent>().Close(UIType.UIRegister);
            Game.Scene.GetComponent<UIComponent>().Create(UIType.UILogin);
        }
    }
}
