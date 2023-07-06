namespace ET.Server
{
    [Event(SceneType.NetInner)]
    public class NetInnerComponentOnReadEvent: AEvent<Scene, NetInnerComponentOnRead>
    {
        protected override async ETTask Run(Scene root, NetInnerComponentOnRead args)
        {
            ActorId actorId = args.ActorId;
            int fromProcess = actorId.Process;
            actorId.Process = root.Fiber.Process;
            object message = args.Message;

            switch (message)
            {
                case IActorLocationRequest iActorRequest:
                {
                    IActorResponse response = await root.GetComponent<ActorInnerComponent>().Call(actorId, iActorRequest, false);
                    actorId.Process = fromProcess;
                    root.GetComponent<ActorOuterComponent>().Send(actorId, response);
                    break;
                }
                case IActorResponse iActorResponse:
                    root.GetComponent<ActorOuterComponent>().HandleIActorResponse(iActorResponse);
                    return;
                case IActorRequest iActorRequest:
                {
                    IActorResponse response = await root.GetComponent<ActorInnerComponent>().Call(actorId, iActorRequest);
                    actorId.Process = fromProcess;
                    root.GetComponent<ActorOuterComponent>().Send(actorId, response);
                    break;
                }
                default:
                {
                    ActorMessageQueue.Instance.Send(actorId, (MessageObject)message);
                    break;
                }
            }

            await ETTask.CompletedTask;
        }
    }
}