using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Battle)]
    public class FrameMessageHandler: AMActorHandler<BattleScene, FrameMessage>
    {
        protected override async ETTask Run(BattleScene battleScene, FrameMessage message)
        {
            OneFrameMessages oneFrameMessages = battleScene.GetComponent<ServerFrameRecvComponent>().Add(message);
            if (oneFrameMessages != null)
            {
                battleScene.FrameBuffer.AddRealFrame(oneFrameMessages);
            }
            
            RoomMessageHelper.BroadCast(battleScene, oneFrameMessages);
            await ETTask.CompletedTask;
        }
    }
}