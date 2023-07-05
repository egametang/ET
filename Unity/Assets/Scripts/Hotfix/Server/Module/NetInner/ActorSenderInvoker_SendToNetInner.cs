namespace ET.Server
{
    [Invoke((long)SceneType.NetInner)]
    public class ActorSenderInvoker_SendToNetInner: AInvokeHandler<ActorSenderInvoker>
    {
        public override void Handle(ActorSenderInvoker args)
        {
            A2NetInner_Message netInnerMessage = A2NetInner_Message.Create();
            netInnerMessage.FromAddress = args.Fiber.Address;
            netInnerMessage.ActorId = args.ActorId;
            netInnerMessage.MessageObject = args.MessageObject;
            // 扔到Net纤程
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.NetInners[args.Fiber.Process];
            ActorMessageQueue.Instance.Send(startSceneConfig.ActorId, netInnerMessage);
        }
    }
}