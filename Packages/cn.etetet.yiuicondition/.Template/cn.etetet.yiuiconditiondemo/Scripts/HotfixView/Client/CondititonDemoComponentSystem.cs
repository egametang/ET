using System;
using System.Collections.Generic;

namespace ET.Client
{
    /// <summary>
    /// Desc    条件测试
    /// </summary>
    [FriendOf(typeof(CondititonDemoComponent))]
    [EntitySystemOf(typeof(CondititonDemoComponent))]
    public static partial class CondititonDemoComponentSystem
    {
        #region ObjectSystem

        [EntitySystem]
        private static void Awake(this CondititonDemoComponent self)
        {
            ConditionMgr.Instance.AddCheckConditionGroupListener(self, "ConditionResult", EConditionGroupId.DemoGroup1);
            self.m_TimerId = self.Root().GetComponent<TimerComponent>().NewRepeatedTimer(2000, ConditionDemoTimerInvokeType.ConditionDemoTimerInvoke, self);
        }

        [YIUIInvoke]
        private static void ConditionResult(this CondititonDemoComponent self, bool arg1, string arg2)
        {
            Log.Error($"条件判断: 结果:{arg1}  失败原因:{arg2}");
        }

        [EntitySystem]
        private static void Destroy(this CondititonDemoComponent self)
        {
            self?.Root()?.GetComponent<TimerComponent>()?.Remove(ref self.m_TimerId);
        }

        [Invoke(ConditionDemoTimerInvokeType.ConditionDemoTimerInvoke)]
        public class TimerInvoke_ConditionDemo : ATimer<CondititonDemoComponent>
        {
            protected override void Run(CondititonDemoComponent self)
            {
                self.DemoValue = (self.DemoValue + 1) % 3;
                Log.Info($"条件demo改变测试:  {self.DemoValue}");
                ConditionMgr.Instance.TriggerListener(EConditionType.Demo);
            }
        }

        #endregion
    }
}
