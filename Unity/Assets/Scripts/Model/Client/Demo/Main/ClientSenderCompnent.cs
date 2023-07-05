namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class ClientSenderCompnent: Entity, IAwake, IDestroy
    {
        public int fiberId;

        public ActorId netClientActorId;

        private EntityRef<ActorInnerComponent> actorInner;

        public ActorInnerComponent ActorInner
        {
            get
            {
                return this.actorInner;
            }
            set
            {
                this.actorInner = value;
            }
        }
    }
}