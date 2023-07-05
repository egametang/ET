namespace ET.Client
{
    [ActorMessageHandler(SceneType.NetClient)]
    public class A2NetClient_MessageHandler: ActorMessageHandler<Scene, A2NetClient_Message>
    {
        protected override async ETTask Run(Scene root, A2NetClient_Message message)
        {
            root.GetComponent<SessionComponent>().Session.Send(message.MessageObject);
            await ETTask.CompletedTask;
        }
    }
}