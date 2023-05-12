using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [FriendOf(typeof(UILSLobbyComponent))]
    public static class UILSLobbyComponentSystem
    {
        [ObjectSystem]
        public class UILSLobbyComponentAwakeSystem: AwakeSystem<UILSLobbyComponent>
        {
            protected override void Awake(UILSLobbyComponent self)
            {
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();

                self.enterMap = rc.Get<GameObject>("EnterMap");
                self.enterMap.GetComponent<Button>().onClick.AddListener(() =>
                {
                    self.EnterMap().Coroutine();
                });
            }
        }

        private static async ETTask EnterMap(this UILSLobbyComponent self)
        {
            await EnterMapHelper.Match(self.ClientScene());
        }
    }
}