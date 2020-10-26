namespace ET
{
    
    public class SessionIdleCheckerComponentAwakeSystem : AwakeSystem<SessionIdleCheckerComponent, int, int, int>
    {
        public override void Awake(SessionIdleCheckerComponent self, int checkInteral, int recvMaxIdleTime, int sendMaxIdleTime)
        {
            self.CheckInterval = checkInteral;
            self.RecvMaxIdleTime = recvMaxIdleTime;
            self.SendMaxIdleTime = sendMaxIdleTime;

            self.RepeatedTimer = TimerComponent.Instance.NewRepeatedTimer(checkInteral, self.Check);
        }
    }
    
    
    public class SessionIdleCheckerComponentLoadSystem : LoadSystem<SessionIdleCheckerComponent>
    {
        public override void Load(SessionIdleCheckerComponent self)
        {
            RepeatedTimer repeatedTimer = TimerComponent.Instance.GetRepeatedTimer(self.RepeatedTimer);
            if (repeatedTimer != null)
            {
                repeatedTimer.Callback = self.Check;
            }
        }
    }
    
    
    public class SessionIdleCheckerComponentDestroySystem : DestroySystem<SessionIdleCheckerComponent>
    {
        public override void Destroy(SessionIdleCheckerComponent self)
        {
            self.CheckInterval = 0;
            self.RecvMaxIdleTime = 0;
            self.SendMaxIdleTime = 0;
            TimerComponent.Instance.Remove(self.RepeatedTimer);
            self.RepeatedTimer = 0;
        }
    }
    
    public static class SessionIdleCheckerComponentSystem
    {
        public static void Check(this SessionIdleCheckerComponent self, bool isTimeOut)
        {
            Session session = self.GetParent<Session>();
            long timeNow = TimeHelper.Now();
            if (timeNow - session.LastRecvTime < self.RecvMaxIdleTime && timeNow - session.LastSendTime < self.SendMaxIdleTime)
            {
                return;
            }
            
            session.Error = ErrorCode.ERR_SessionSendOrRecvTimeout;
            session.Dispose();
        }
    }
    
    public class SessionIdleCheckerComponent: Entity
    {
        public int CheckInterval;
        public int RecvMaxIdleTime;
        public int SendMaxIdleTime;
        public long RepeatedTimer;
    }
}