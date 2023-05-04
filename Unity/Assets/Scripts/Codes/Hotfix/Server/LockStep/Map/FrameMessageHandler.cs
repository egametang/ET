using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class FrameMessageHandler: AMActorHandler<Room, FrameMessage>
    {
        protected override async ETTask Run(Room room, FrameMessage message)
        {
            FrameBuffer frameBuffer = room.FrameBuffer;
            if (message.Frame < frameBuffer.RealFrame)  // 小于RealFrame，丢弃
            {
                return;
            }
            OneFrameMessages oneFrameMessages = frameBuffer.GetFrame(message.Frame);
            oneFrameMessages.Inputs[message.PlayerId] = message.Input;

            if (message.Frame % (1000 / LSConstValue.UpdateInterval) == 0)
            {
                long nowFrameTime = room.FixedTimeCounter.FrameTime(message.Frame);
                int diffTime = (int)(nowFrameTime - TimeHelper.ServerFrameTime());

                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(message.PlayerId, new Room2C_AdjustUpdateTime() {DiffTime = diffTime});
            }
            await ETTask.CompletedTask;
        }
    }
}