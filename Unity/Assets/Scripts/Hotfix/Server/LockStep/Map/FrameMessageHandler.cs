using System;
using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class FrameMessageHandler: AMActorHandler<Room, FrameMessage>
    {
        protected override async ETTask Run(Room room, FrameMessage message)
        {
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            if (message.Frame % (1000 / LSConstValue.UpdateInterval) == 0)
            {
                long nowFrameTime = room.FixedTimeCounter.FrameTime(message.Frame);
                int diffTime = (int)(nowFrameTime - TimeHelper.ServerFrameTime());

                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(message.PlayerId, new Room2C_AdjustUpdateTime() {DiffTime = diffTime});
            }

            if (message.Frame < room.RealFrame)  // 小于RealFrame，丢弃
            {
                Log.Warning($"FrameMessage discard: {message}");
                return;
            }
            
            OneFrameMessages oneFrameMessages = frameBuffer[message.Frame];
            if (oneFrameMessages == null)
            {
                Log.Error($"FrameMessageHandler get frame is null: {message.Frame}, max frame: {frameBuffer.MaxFrame}");
                return;
            }
            oneFrameMessages.Frame = message.Frame;
            oneFrameMessages.Inputs[message.PlayerId] = message.Input;


            await ETTask.CompletedTask;
        }
    }
}