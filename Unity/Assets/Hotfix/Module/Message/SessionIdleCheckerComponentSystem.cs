namespace ET
{
    [ObjectSystem]
    public class SessionIdleCheckerComponentAwakeSystem: AwakeSystem<SessionIdleCheckerComponent, int>
    {
        public override void Awake(SessionIdleCheckerComponent self, int checkInteral)
        {
            self.RepeatedTimer = TimerComponent.Instance.NewRepeatedTimer(checkInteral, self.Check);
        }
    }

    [ObjectSystem]
    public class SessionIdleCheckerComponentDestroySystem: DestroySystem<SessionIdleCheckerComponent>
    {
        public override void Destroy(SessionIdleCheckerComponent self)
        {
            TimerComponent.Instance.Remove(ref self.RepeatedTimer);
        }
    }

    public static class SessionIdleCheckerComponentSystem
    {
        public static void Check(this SessionIdleCheckerComponent self)
        {
            Session session = self.GetParent<Session>();
            long timeNow = TimeHelper.ClientNow();
            
            if (timeNow - session.LastRecvTime < 30 * 1000 && timeNow - session.LastSendTime < 30 * 1000)
            {
                return;
            }
            
            Log.Info($"session timeout: {session.Id} {timeNow} {session.LastRecvTime} {session.LastSendTime} {timeNow - session.LastRecvTime} {timeNow - session.LastSendTime}");
            session.Error = ErrorCode.ERR_SessionSendOrRecvTimeout;

            session.Dispose();
        }
    }
}