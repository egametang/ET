using System.Collections.Generic;

namespace ET.Server
{
    [UniqueId(0, 100)]
    public static class LocationType
    {
        public const int Unit = 0;
        public const int Player = 1;
        public const int Friend = 2;
        public const int Chat = 3;
        public const int Max = 100;
    }
    
    [ChildOf(typeof(LocationOneType))]
    public class LockInfo: Entity, IAwake<long, CoroutineLock>, IDestroy
    {
        public long LockInstanceId;

        public CoroutineLock CoroutineLock;
    }

    [ChildOf(typeof(LocationManagerComoponent))]
    public class LocationOneType: Entity, IAwake<int>
    {
        public int LocationType;
        
        public readonly Dictionary<long, long> locations = new Dictionary<long, long>();

        public readonly Dictionary<long, LockInfo> lockInfos = new Dictionary<long, LockInfo>();
    }

    [ComponentOf(typeof(Scene))]
    public class LocationManagerComoponent: Entity, IAwake
    {
        public LocationOneType[] LocationOneTypes = new LocationOneType[LocationType.Max];
    }
}