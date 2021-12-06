using System;

namespace ET
{
    [Timer(TimerType.SessionAcceptTimeout)]
    public class SessionAcceptTimeout: ATimer<SessionAcceptTimeoutComponent>
    {
        public override void Run(SessionAcceptTimeoutComponent self)
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
        public override void Awake(SessionAcceptTimeoutComponent self)
        {
            self.Timer = TimerComponent.Instance.NewOnceTimer(TimeHelper.ServerNow() + 5000, TimerType.SessionAcceptTimeout, self);
        }
    }

    [ObjectSystem]
    public class SessionAcceptTimeoutComponentDestroySystem: DestroySystem<SessionAcceptTimeoutComponent>
    {
        public override void Destroy(SessionAcceptTimeoutComponent self)
        {
            TimerComponent.Instance.Remove(ref self.Timer);
        }
    }
}