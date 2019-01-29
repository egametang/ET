using System;
using System.Net;
using ETModel;
using UnityEngine;
using UnityEngine.UI;

namespace ETHotfix
{
    [ObjectSystem]
    public class UIStartComponentSystem : AwakeSystem<UIStartComponent>
    {
        public override void Awake(UIStartComponent self)
        {
            self.Awake();
        }
    }

    public class UIStartComponent : Component
    {
        private GameObject account;
        private GameObject password;
        private GameObject loginBtn;

        public void Awake()
        {
            ReferenceCollector rc = this.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            loginBtn = rc.Get<GameObject>("BtnEnter");
            loginBtn.GetComponent<Button>().onClick.Add(OnLogin);
            this.account = rc.Get<GameObject>("FieldUserName");
            this.password = rc.Get<GameObject>("FieldPassWord");
        }

        public async void OnLogin()
        {
            try {
                String accountValue = account.transform.GetComponent<InputField>().text;
                string passwordValue = password.transform.GetComponent<InputField>().text;
                Debug.Log("username:" + accountValue);
                Debug.Log("passWord:" + passwordValue);
                Debug.Log("start enter");
                

                TestLoginRequest request = new TestLoginRequest();
                request.UsreName = accountValue;
                request.Password = passwordValue;

                // 创建一个ETHotfix层的Session, 并且保存到ETHotfix.SessionComponent中
                addSession: if (Game.Scene.GetComponent<SessionComponent>() == null) {
                  
                    IPEndPoint address = NetworkHelper.ToIPEndPoint(GlobalConfigComponent.Instance.GlobalProto.Address);
                    ETModel.Session session = ETModel.Game.Scene.GetComponent<NetOuterComponent>().Create(address);
                    ETModel.Game.Scene.AddComponent<ETModel.SessionComponent>().Session = session;
                    Game.Scene.AddComponent<SessionComponent>().Session = ComponentFactory.Create<Session, ETModel.Session>(session);
                }
                else {
                    //断线重连的逻辑
                    ETModel.Session sesson = ETModel.Game.Scene.GetComponent<ETModel.SessionComponent>().Session;
                    if (sesson.IsDisposed) {
                        ETModel.Game.Scene.RemoveComponent<ETModel.SessionComponent>();
                        Game.Scene.RemoveComponent<SessionComponent>();
                        goto addSession;
                    }
                }

                TestLoginResponse response = (TestLoginResponse)await SessionComponent.Instance.Session.Call(request);
                Debug.Log(response.Message);
            }
            catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }
    }
}
