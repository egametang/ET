using System;

namespace ET
{
    [EntitySystemOf(typeof(SessionIdleCheckerComponent))]
    [FriendOf(typeof(SessionIdleCheckerComponent))]
    public static partial class SessionIdleCheckerComponentSystem
    {
        [Invoke(TimerInvokeType.SessionIdleChecker)]
        public class SessionIdleChecker: ATimer<SessionIdleCheckerComponent>
        {
            protected override void Run(SessionIdleCheckerComponent self)
            {
                try
                {
                    self.Check();
                }
                catch (Exception e)
                {
                    Log.Error($"session idle checker timer error: {self.Id}\n{e}");
                }
            }
        }
    
        [EntitySystem]
        private static void Awake(this SessionIdleCheckerComponent self)
        {
            self.RepeatedTimer = self.Fiber().TimerComponent.NewRepeatedTimer(CheckInteral, TimerInvokeType.SessionIdleChecker, self);
        }
        
        [EntitySystem]
        private static void Destroy(this SessionIdleCheckerComponent self)
        {
            self.Fiber().TimerComponent?.Remove(ref self.RepeatedTimer);
        }
        
        public const int CheckInteral = 2000;

        private static void Check(this SessionIdleCheckerComponent self)
        {
            Session session = self.GetParent<Session>();
            long timeNow = self.Fiber().TimeInfo.ClientNow();

            if (timeNow - session.LastRecvTime < ConstValue.SessionTimeoutTime && timeNow - session.LastSendTime < ConstValue.SessionTimeoutTime)
            {
                return;
            }

            Log.Info($"session timeout: {session.Id} {timeNow} {session.LastRecvTime} {session.LastSendTime} {timeNow - session.LastRecvTime} {timeNow - session.LastSendTime}");
            session.Error = ErrorCore.ERR_SessionSendOrRecvTimeout;

            session.Dispose();
        }
    }
}