using System.Collections.Generic;

namespace ET.Server
{
    [ChildOf(typeof(LocationOneType))]
    public class LockInfo: Entity, IAwake<ActorId, CoroutineLock>, IDestroy
    {
        public ActorId LockActorId;

        public CoroutineLock CoroutineLock
        {
            get;
            set;
        }
    }

    [ChildOf(typeof(LocationManagerComoponent))]
    public class LocationOneType: Entity, IAwake
    {
        public readonly Dictionary<long, ActorId> locations = new();

        public readonly Dictionary<long, EntityRef<LockInfo>> lockInfos = new();
    }

    [ComponentOf(typeof(Scene))]
    public class LocationManagerComoponent: Entity, IAwake
    {
    }
}