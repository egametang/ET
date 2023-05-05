namespace ET.Server
{
    [FriendOf(typeof(RouterNode))]
    public static class RouterNodeSystem
    {
        [ObjectSystem]
        public class RouterNodeAwakeSystem: AwakeSystem<RouterNode>
        {
            protected override void Awake(RouterNode self)
            {
                long timeNow = TimeHelper.ServerNow();
                self.LastRecvInnerTime = timeNow;
                self.LastRecvOuterTime = timeNow;
                self.OuterIpEndPoint = null;
                self.InnerIpEndPoint = null;
                self.RouterSyncCount = 0;
                self.OuterConn = 0;
                self.InnerConn = 0;
            }
        }

        [ObjectSystem]
        public class RouterNodeDestroySystem: DestroySystem<RouterNode>
        {
            protected override void Destroy(RouterNode self)
            {
                self.OuterConn = 0;
                self.InnerConn = 0;
                self.LastRecvInnerTime = 0;
                self.LastRecvOuterTime = 0;
                self.OuterIpEndPoint = null;
                self.InnerIpEndPoint = null;
                self.InnerAddress = null;
                self.RouterSyncCount = 0;
                self.SyncCount = 0;
            }
        }
        
        public static bool CheckOuterCount(this RouterNode self, long timeNow)
        {
            if (self.LastCheckTime == 0)
            {
                self.LastCheckTime = timeNow;
            }
            
            if (timeNow - self.LastCheckTime > 1000)
            {
                //Log.Debug($"router recv packet per second: {self.LimitCountPerSecond}");
                self.LimitCountPerSecond = 0;
                self.LastCheckTime = timeNow;
            }

            if (++self.LimitCountPerSecond > 1000)
            {
                return false;
            }

            return true;
        }
    }
}