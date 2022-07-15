using System.Collections.Generic;

namespace ET.Server
{
    [ChildType(typeof(ActorLocationSender))]
    [ComponentOf(typeof(Scene))]
    public class ActorLocationSenderComponent: Entity, IAwake, IDestroy
    {
        public const long TIMEOUT_TIME = 60 * 1000;

        public static ActorLocationSenderComponent Instance { get; set; }

        public long CheckTimer;
    }
}