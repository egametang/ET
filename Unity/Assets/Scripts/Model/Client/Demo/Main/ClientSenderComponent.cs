using System.Collections.Generic;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class ClientSenderComponent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        //可以写成属性，但是private set在System同样set不了
        public ActorId netClientActorId;
    }
}