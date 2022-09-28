using System;

namespace ET
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
                Log.Error($"move timer error: {self.Id}\n{e}");
            }
        }
    }
    
    [ObjectSystem]
    public class SessionIdleCheckerComponentAwakeSystem: AwakeSystem<SessionIdleCheckerComponent>
    {
        protected override void Awake(SessionIdleCheckerComponent self)
        {
            self.RepeatedTimer = TimerComponent.Instance.NewRepeatedTimer(SessionIdleCheckerComponentSystem.CheckInteral, TimerInvokeType.SessionIdleChecker, self);
        }
    }

    [ObjectSystem]
    public class SessionIdleCheckerComponentDestroySystem: DestroySystem<SessionIdleCheckerComponent>
    {
        protected override void Destroy(SessionIdleCheckerComponent self)
        {
            TimerComponent.Instance?.Remove(ref self.RepeatedTimer);
        }
    }

    public static class SessionIdleCheckerComponentSystem
    {
        public const int CheckInteral = 2000;
        
        public static void Check(this SessionIdleCheckerComponent self)
        {
            Session session = self.GetParent<Session>();
            long timeNow = TimeHelper.ClientNow();

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