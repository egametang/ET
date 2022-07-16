namespace ET.Server
{
    public class ActorMessageHandlerAttribute: BaseAttribute
    {
        public SceneType SceneType { get; }

        public ActorMessageHandlerAttribute(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
}