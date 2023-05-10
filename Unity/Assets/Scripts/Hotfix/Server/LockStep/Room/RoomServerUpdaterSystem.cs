using System;
using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(RoomServerUpdater))]
    public static class RoomServerUpdaterSystem
    {
        [ObjectSystem]
        public class UpdateSystem: UpdateSystem<RoomServerUpdater>
        {
            protected override void Update(RoomServerUpdater self)
            {
                self.Update();
            }
        }
        
        private static void Update(this RoomServerUpdater self)
        {
            Room room = self.GetParent<Room>();
            long timeNow = TimeHelper.ServerFrameTime();
            
            
            int frame = room.RealFrame + 1;
            if (timeNow < room.FixedTimeCounter.FrameTime(frame))
            {
                return;
            }

            OneFrameInputs oneFrameInputs = self.GetOneFrameMessage(frame);
            ++room.RealFrame;

            OneFrameInputs sendInput = new();
            oneFrameInputs.CopyTo(sendInput);

            RoomMessageHelper.BroadCast(room, sendInput);
            
            room.Update(oneFrameInputs, frame);
        }

        private static OneFrameInputs GetOneFrameMessage(this RoomServerUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            OneFrameInputs oneFrameInputs = frameBuffer[frame];
            if (oneFrameInputs == null)
            {
                throw new Exception($"get frame is null: {frame}, max frame: {frameBuffer.MaxFrame}");
            }
            
            frameBuffer.MoveForward(frame);
            
            if (oneFrameInputs.Inputs.Count == LSConstValue.MatchCount)
            {
                return oneFrameInputs;
            }

            OneFrameInputs preFrameInputs = frameBuffer[frame - 1];
            
            // 有人输入的消息没过来，给他使用上一帧的操作
            foreach (long playerId in room.PlayerIds)
            {
                if (oneFrameInputs.Inputs.ContainsKey(playerId))
                {
                    continue;
                }

                if (preFrameInputs != null && preFrameInputs.Inputs.TryGetValue(playerId, out LSInput input))
                {
                    // 使用上一帧的输入
                    oneFrameInputs.Inputs[playerId] = input;
                }
                else
                {
                    oneFrameInputs.Inputs[playerId] = new LSInput();
                }
            }

            return oneFrameInputs;
        }
    }
}