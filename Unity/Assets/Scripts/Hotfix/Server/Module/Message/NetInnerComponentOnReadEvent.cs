namespace ET.Server
{
    [Event(SceneType.NetInner)]
    public class NetInnerComponentOnReadEvent: AEvent<Scene, NetInnerComponentOnRead>
    {
        protected override async ETTask Run(Scene root, NetInnerComponentOnRead args)
        {
            ActorId actorId = args.ActorId;
            object message = args.Message;

            if (message is IActorResponse iActorResponse)
            {
                root.GetComponent<ActorOuterComponent>().HandleIActorResponse(iActorResponse);
                return;
            }
            
            ActorMessageQueue.Instance.Send(actorId, (MessageObject)message);

            await ETTask.CompletedTask;
        }
    }
}