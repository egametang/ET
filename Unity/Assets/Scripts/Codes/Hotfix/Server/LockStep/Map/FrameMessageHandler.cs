namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class FrameMessageHandler: AMActorHandler<Scene, FrameMessage>
    {
        protected override async ETTask Run(Scene roomScene, FrameMessage message)
        {
            OneFrameMessages oneFrameMessages = roomScene.GetComponent<ServerFrameRecvComponent>().Add(message);
            if (oneFrameMessages != null)
            {
                BattleScene battleScene = roomScene.GetComponent<BattleScene>();
                battleScene.FrameBuffer.AddFrameMessage(oneFrameMessages);
            }
            
            RoomMessageHelper.BroadCast(roomScene, oneFrameMessages);
            await ETTask.CompletedTask;
        }
    }
}