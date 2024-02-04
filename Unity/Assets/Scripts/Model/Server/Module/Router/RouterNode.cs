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
        public string InnerAddress;
        public IPEndPoint InnerIpEndPoint;
        public IPEndPoint OuterIpEndPoint;
        public IPEndPoint SyncIpEndPoint;
        public IKcpTransport KcpTransport;

        public uint OuterConn
        {
            get
            {
                return (uint)this.Id;
            }
        }
        public uint InnerConn;
        public uint ConnectId;
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