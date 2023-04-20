namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class OneFrameMessagesHandler: AMHandler<OneFrameMessages>
    {
        protected override async ETTask Run(Session session, OneFrameMessages message)
        {
            FrameBuffer frameBuffer = session.DomainScene().GetComponent<BattleScene>().FrameBuffer;
            frameBuffer.AddRealFrame(message);
            
            PingComponent pingComponent = session.GetComponent<PingComponent>();
            frameBuffer.PredictionCount = (int) (pingComponent.Ping / 2f / LSConstValue.UpdateInterval) + 1;
            await ETTask.CompletedTask;
        }
    }
}