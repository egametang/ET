using System;
using System.Collections.Generic;

namespace ET.Server
{
    public static partial class FrameMessageHandler
    {
        [ActorMessageHandler(SceneType.Room)]
        private static async ETTask Run(Room room, FrameMessage message)
        {
            FrameBuffer frameBuffer = room.FrameBuffer;
            
            if (message.Frame % (1000 / LSConstValue.UpdateInterval) == 0)
            {
                long nowFrameTime = room.FixedTimeCounter.FrameTime(message.Frame);
                int diffTime = (int)(nowFrameTime - TimeHelper.ServerFrameTime());

                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(message.PlayerId, new Room2C_AdjustUpdateTime() {DiffTime = diffTime});
            }

            if (message.Frame < room.AuthorityFrame)  // 小于AuthorityFrame，丢弃
            {
                Log.Warning($"FrameMessage < AuthorityFrame discard: {message}");
                return;
            }

            if (message.Frame > room.AuthorityFrame + 10)  // 大于AuthorityFrame + 10，丢弃
            {
                Log.Warning($"FrameMessage > AuthorityFrame + 10 discard: {message}");
                return;
            }
            
            OneFrameInputs oneFrameInputs = frameBuffer.FrameInputs(message.Frame);
            if (oneFrameInputs == null)
            {
                Log.Error($"FrameMessageHandler get frame is null: {message.Frame}, max frame: {frameBuffer.MaxFrame}");
                return;
            }
            oneFrameInputs.Inputs[message.PlayerId] = message.Input;


            await ETTask.CompletedTask;
        }
    }
}