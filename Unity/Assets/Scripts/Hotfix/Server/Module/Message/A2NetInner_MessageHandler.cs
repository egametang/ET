namespace ET.Server
{
    [ActorMessageHandler(SceneType.NetInner)]
    public class A2NetInner_MessageHandler: ActorMessageHandler<Fiber, A2NetInner_Message>
    {
        protected override async ETTask Run(Fiber fiber, A2NetInner_Message message)
        {
            int process = message.ActorId.Process;
            Session session = fiber.GetComponent<NetInnerComponent>().Get(process);
            ActorId actorId = message.ActorId;
            actorId.Address = message.FromAddress;
            session.Send(actorId, message.MessageObject);
            await ETTask.CompletedTask;
        }
    }
}