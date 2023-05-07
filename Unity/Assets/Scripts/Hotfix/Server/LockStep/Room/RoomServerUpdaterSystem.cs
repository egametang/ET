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

            OneFrameMessages oneFrameMessages = self.GetOneFrameMessage(frame);
            ++room.RealFrame;

            OneFrameMessages sendMessage = new();
            oneFrameMessages.CopyTo(sendMessage);

            RoomMessageHelper.BroadCast(room, sendMessage);
            
            room.Update(oneFrameMessages, frame);
        }

        private static OneFrameMessages GetOneFrameMessage(this RoomServerUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            OneFrameMessages oneFrameMessages = frameBuffer[frame];
            if (oneFrameMessages == null)
            {
                throw new Exception($"get frame is null: {frame}, max frame: {frameBuffer.MaxFrame}");
            }
            
            frameBuffer.MoveForward(frame);
            
            oneFrameMessages.Frame = frame;
            if (oneFrameMessages.Inputs.Count == LSConstValue.MatchCount)
            {
                return oneFrameMessages;
            }

            OneFrameMessages preFrameMessages = frameBuffer[frame - 1];
            
            // 有人输入的消息没过来，给他使用上一帧的操作
            foreach (long playerId in room.PlayerIds)
            {
                if (oneFrameMessages.Inputs.ContainsKey(playerId))
                {
                    continue;
                }

                if (preFrameMessages != null && preFrameMessages.Inputs.TryGetValue(playerId, out LSInput input))
                {
                    // 使用上一帧的输入
                    oneFrameMessages.Inputs[playerId] = input;
                }
                else
                {
                    oneFrameMessages.Inputs[playerId] = new LSInput();
                }
            }

            return oneFrameMessages;
        }
    }
}