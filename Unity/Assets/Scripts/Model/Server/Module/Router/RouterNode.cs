using System.Net;

namespace ET.Server
{
    public enum RouterStatus
    {
        Sync,
        Msg,
    }

    [ChildOf(typeof(RouterComponent))]
    public class RouterNode: Entity, IDestroy, IAwake
    {
        public uint ConnectId;
        public string InnerAddress;
        public IPEndPoint InnerIpEndPoint;
        public IPEndPoint OuterIpEndPoint;
        public IPEndPoint SyncIpEndPoint;
        public uint OuterConn;
        public uint InnerConn;
        public long LastRecvOuterTime;
        public long LastRecvInnerTime;

        public int RouterSyncCount;
        public int SyncCount;

#region 限制外网消息数量，一秒最多50个包

        public long LastCheckTime;
        public int LimitCountPerSecond;

#endregion

        public RouterStatus Status;
    }
}