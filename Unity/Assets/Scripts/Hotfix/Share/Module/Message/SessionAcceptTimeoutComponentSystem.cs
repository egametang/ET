using System;

namespace ET
{
    [Invoke(TimerInvokeType.SessionAcceptTimeout)]
    public class SessionAcceptTimeout: ATimer<SessionAcceptTimeoutComponent>
    {
        protected override void Run(SessionAcceptTimeoutComponent self)
        {
            try
            {
                self.Parent.Dispose();
            }
            catch (Exception e)
            {
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    
    [ObjectSystem]
    public class SessionAcceptTimeoutComponentAwakeSystem: AwakeSystem<SessionAcceptTimeoutComponent>
    {
        protected override void Awake(SessionAcceptTimeoutComponent self)
        {
            self.Timer = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 5000, TimerInvokeType.SessionAcceptTimeout, self);
        }
    }

    [ObjectSystem]
    public class SessionAcceptTimeoutComponentDestroySystem: DestroySystem<SessionAcceptTimeoutComponent>
    {
        protected override void Destroy(SessionAcceptTimeoutComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }
}