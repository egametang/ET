using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class FrameMessageHandler: AMActorHandler<Room, FrameMessage>
    {
        protected override async ETTask Run(Room room, FrameMessage message)
        {
            OneFrameMessages oneFrameMessages = room.GetComponent<ServerFrameRecvComponent>().Add(message);
            if (oneFrameMessages != null)
            {
                room.FrameBuffer.AddRealFrame(oneFrameMessages);
            }
            
            RoomMessageHelper.BroadCast(room, oneFrameMessages);
            await ETTask.CompletedTask;
        }
    }
}