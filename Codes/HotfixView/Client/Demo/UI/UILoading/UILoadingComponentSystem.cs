﻿using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ObjectSystem]
    public class UiLoadingComponentAwakeSystem : AwakeSystem<UILoadingComponent>
    {
        protected override void Awake(UILoadingComponent self)
        {
            self.text = self.GetParent<UI>().GameObject.Get<GameObject>("Text").GetComponent<Text>();
            self.StartAsync().Coroutine();
        }
    }

    public static class UiLoadingComponentSystem
    {
        public static async ETTask StartAsync(this UILoadingComponent self)
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