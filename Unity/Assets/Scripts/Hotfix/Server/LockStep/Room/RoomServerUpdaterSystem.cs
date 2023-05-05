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
            
            
            int frame = frameBuffer.RealFrame + 1;
            if (timeNow < room.FixedTimeCounter.FrameTime(frame))
            {
                return;
            }
            
            OneFrameMessages oneFrameMessages = frameBuffer.GetFrame(frame);
            if (oneFrameMessages.Inputs.Count != LSConstValue.MatchCount)
            {
                return;
            }
            ++frameBuffer.RealFrame;
            
            OneFrameMessages sendMessage = NetServices.Instance.FetchMessage<OneFrameMessages>();
            
            oneFrameMessages.CopyTo(sendMessage);
            oneFrameMessages.Inputs.Clear();
            oneFrameMessages.Frame = 0;
            
            RoomMessageHelper.BroadCast(room, sendMessage);
        }
    }
}