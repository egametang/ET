using System.Collections.Generic;

namespace ET.Server
{
    public class LockInfo: Entity, IAwake<long, CoroutineLock>, IDestroy
    {
        public long LockInstanceId;

        public CoroutineLock CoroutineLock;
    }

    [ChildType(typeof(LockInfo))]
    [ComponentOf(typeof(Scene))]
    public class LocationComponent: Entity, IAwake
    {
        public readonly Dictionary<long, long> locations = new Dictionary<long, long>();

        public readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();
    }
}