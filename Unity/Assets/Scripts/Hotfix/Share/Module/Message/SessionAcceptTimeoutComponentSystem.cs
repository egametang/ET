using System;

namespace ET
{
    [EntitySystemOf(typeof(SessionAcceptTimeoutComponent))]
    [FriendOf(typeof(SessionAcceptTimeoutComponent))]
    public static partial class SessionAcceptTimeoutComponentHelper
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
        
        [EntitySystem]
        private static void Awake(this SessionAcceptTimeoutComponent self)
        {
            self.Timer = self.Root().GetComponent<TimerComponent>().NewOnceTimer(TimeInfo.Instance.ServerNow() + 5000, TimerInvokeType.SessionAcceptTimeout, self);
        }
        
        [EntitySystem]
        private static void Destroy(this SessionAcceptTimeoutComponent self)
        {
            self.Root().GetComponent<TimerComponent>()?.Remove(ref self.Timer);
        }
        
    }
    
    
}