using UnityEngine;
using UnityEngine.UI;

namespace ET
{
    public class UiLoadingComponentAwakeSystem : AwakeSystem<UILoadingComponent>
    {
        public override void Awake(UILoadingComponent self)
        {
            self.text = self.GetParent<UI>().GameObject.Get<GameObject>("Text").GetComponent<Text>();
            self.StartAsync().Coroutine();
        }
    }

    public static class UiLoadingComponentSystem
    {
        public static async ETVoid StartAsync(this UILoadingComponent self)
        {
            long instanceId = self.InstanceId;
            while (true)
            {
                await TimerComponent.Instance.WaitAsync(1000);

                if (self.InstanceId != instanceId)
                {
                    return;
                }
            }
        }
    }

}