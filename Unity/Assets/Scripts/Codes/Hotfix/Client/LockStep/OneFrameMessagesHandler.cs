namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class OneFrameMessagesHandler: AMHandler<OneFrameMessages>
    {
        protected override async ETTask Run(Session session, OneFrameMessages message)
        {
            session.DomainScene().GetComponent<BattleScene>().FrameBuffer.AddFrameMessage(message);
            await ETTask.CompletedTask;
        }
    }
}