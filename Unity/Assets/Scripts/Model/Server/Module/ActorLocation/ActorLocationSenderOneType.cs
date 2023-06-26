using System.Collections.Generic;

namespace ET.Server
{
    
    [ChildOf(typeof(ActorLocationSenderComponent))]
    public class ActorLocationSenderOneType: Entity, IAwake<int>, IDestroy
    {
        public const long TIMEOUT_TIME = 60 * 1000;

        public long CheckTimer;

        public int LocationType;
    }
    
    
    [ComponentOf(typeof(Fiber))]
    public class ActorLocationSenderComponent: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 60 * 1000;

        public long CheckTimer;

        public ActorLocationSenderOneType[] ActorLocationSenderComponents = new ActorLocationSenderOneType[LocationType.Max];
    }
}
