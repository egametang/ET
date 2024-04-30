using System.Collections.Generic;

namespace ET.Server
{
    
    [ChildOf(typeof(MessageLocationSenderComponent))]
    public class MessageLocationSenderOneType: Entity, IAwake<int>, IDestroy
    {
        public const long TIMEOUT_TIME = 60 * 1000;

        public long CheckTimer;

        public int LocationType;
    }
    
    
    [ComponentOf(typeof(Scene))]
    public class MessageLocationSenderComponent: Entity, IAwake
    {
        public const long TIMEOUT_TIME = 60 * 1000;

        public long CheckTimer;

        public MessageLocationSenderOneType[] messageLocationSenders = new MessageLocationSenderOneType[LocationType.Max];
    }
}
