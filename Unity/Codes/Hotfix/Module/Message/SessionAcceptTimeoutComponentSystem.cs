﻿namespace ET
{
    [ObjectSystem]
    public class SessionAcceptTimeoutComponentAwakeSystem: AwakeSystem<SessionAcceptTimeoutComponent>
    {
        public override void Awake(SessionAcceptTimeoutComponent self)
        {
            //self.Timer = TimerComponent.Instance.NewOnceTimer(5000, () => { self.Parent.Dispose(); });
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
    
    public static class SessionAcceptTimeoutComponentSystem
    {
        
    }
}