using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    public static class UILSRoomComponentSystem
    {
        public class AwakeSystem : AwakeSystem<UILSRoomComponent>
        {
            protected override void Awake(UILSRoomComponent self)
            {
                ReferenceCollector rc = self.GetParent<UI>().GameObject.GetComponent<ReferenceCollector>();
                self.saveReplay = rc.Get<GameObject>("SaveReplay");
                self.saveName = rc.Get<GameObject>("SaveName").GetComponent<InputField>();
				
                self.saveReplay.GetComponent<Button>().onClick.AddListener(()=> { self.OnSaveReplay().Coroutine(); });
            }
        }

        private static async ETTask OnSaveReplay(this UILSRoomComponent self)
        {
            string name = self.saveName.text;
            
            LSHelper.SaveReplay(self.Room(), name);

            await ETTask.CompletedTask;
        }
    }
}