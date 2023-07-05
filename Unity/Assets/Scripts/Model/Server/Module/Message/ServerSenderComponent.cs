namespace ET.Server
{
    [ComponentOf(typeof(Scene))]
    public class ServerSenderComponent: Entity, IAwake
    {
        private EntityRef<ActorInnerComponent> actorInnerComponent;

        public ActorInnerComponent ActorInnerComponent
        {
            get
            {
                return this.actorInnerComponent;
            }
            set
            {
                this.actorInnerComponent = value;
            }
        }
    }
}