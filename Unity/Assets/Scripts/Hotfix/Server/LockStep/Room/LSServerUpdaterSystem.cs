using System;
using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(LSServerUpdater))]
    public static class LSServerUpdaterSystem
    {
        [EntitySystem]
        public class UpdateSystem: UpdateSystem<LSServerUpdater>
        {
            protected override void Update(LSServerUpdater self)
            {
                self.Update();
            }
        }
        
        private static void Update(this LSServerUpdater self)
        {
            Room room = self.GetParent<Room>();
            long timeNow = TimeHelper.ServerFrameTime();
            
            
            int frame = room.AuthorityFrame + 1;
            if (timeNow < room.FixedTimeCounter.FrameTime(frame))
            {
                return;
            }

            OneFrameInputs oneFrameInputs = self.GetOneFrameMessage(frame);
            ++room.AuthorityFrame;

            OneFrameInputs sendInput = new();
            oneFrameInputs.CopyTo(sendInput);

            RoomMessageHelper.BroadCast(room, sendInput);
            
            room.Update(oneFrameInputs);
        }

        private static OneFrameInputs GetOneFrameMessage(this LSServerUpdater self, int frame)
        {
            Room room = self.GetParent<Room>();
            FrameBuffer frameBuffer = room.FrameBuffer;
            OneFrameInputs oneFrameInputs = frameBuffer.FrameInputs(frame);
            frameBuffer.MoveForward(frame);
            
            if (oneFrameInputs.Inputs.Count == LSConstValue.MatchCount)
            {
                return oneFrameInputs;
            }

            OneFrameInputs preFrameInputs = null;
            if (frameBuffer.CheckFrame(frame - 1))
            {
                preFrameInputs = frameBuffer.FrameInputs(frame - 1);    
            }

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