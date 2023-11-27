using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [EntitySystemOf(typeof(UILSLoginComponent))]
    [FriendOf(typeof(UILoginComponent))]
    [FriendOfAttribute(typeof(ET.Client.UILSLoginComponent))]
    public static partial class UILSLoginComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UILSLoginComponent self)
        {
            ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
            self.loginBtn = rc.Get<GameObject>("LoginBtn");

            self.loginBtn.GetComponent<Button>().onClick.AddListener(() => { self.OnLogin(); });
            self.account = rc.Get<GameObject>("Account");
            self.password = rc.Get<GameObject>("Password");
        }


        public static void OnLogin(this UILSLoginComponent self)
        {
            LoginHelper.Login(
                self.Root(),
                self.account.GetComponent<InputField>().text,
                self.password.GetComponent<InputField>().text).Coroutine();
        }
    }
}
