namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class ClientSenderCompnent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        public ActorId netClientActorId;

        private EntityRef<ActorSenderComponent> actorSender;

        public ActorSenderComponent ActorSender
        {
            get
            {
                return this.actorSender;
            }
            set
            {
                this.actorSender = value;
            }
        }
    }
}