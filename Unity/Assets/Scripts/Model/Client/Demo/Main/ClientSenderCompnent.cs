namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class ClientSenderCompnent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        public ActorId netClientActorId;
    }
}