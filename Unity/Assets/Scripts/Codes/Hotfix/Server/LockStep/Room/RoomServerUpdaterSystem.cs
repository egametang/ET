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
            FrameBuffer frameBuffer = room.FrameBuffer;
            long timeNow = TimeHelper.ServerFrameTime();
            if (!room.FixedTimeCounter.IsTimeout(timeNow, frameBuffer.NowFrame))
            {
                return;
            }

            if (!self.FrameMessages.TryGetValue(self.NowFrame, out OneFrameMessages oneFrameMessages))
            {
                return;
            }

            if (oneFrameMessages.Inputs.Count != LSConstValue.MatchCount)
            {
                return;
            }
            self.FrameMessages.Remove(oneFrameMessages.Frame);
            ++self.NowFrame;
            
            RoomMessageHelper.BroadCast(room, oneFrameMessages);
        }
        
        
        public static void Add(this RoomServerUpdater self, FrameMessage message)
        {
            if (message.Frame < self.NowFrame)
            {
                return;
            }
            
            OneFrameMessages oneFrameMessages;
            if (!self.FrameMessages.TryGetValue(message.Frame, out oneFrameMessages))
            {
                oneFrameMessages = new OneFrameMessages
                {
                    Frame = message.Frame,
                };
                self.FrameMessages.Add(oneFrameMessages.Frame, oneFrameMessages);
            }
            oneFrameMessages.Inputs[message.PlayerId] = message.Input;
        }
    }
}