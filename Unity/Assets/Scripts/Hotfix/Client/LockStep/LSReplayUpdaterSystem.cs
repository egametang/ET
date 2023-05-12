using System;

namespace ET.Client
{
    [FriendOf(typeof(LSReplayUpdater))]
    public static class LSReplayComponentSystem
    {
        [ObjectSystem]
        public class UpdateSystem: UpdateSystem<LSReplayUpdater>
        {
            protected override void Update(LSReplayUpdater self)
            {
                self.Update();
            }
        }

        private static void Update(this LSReplayUpdater self)
        {
            Room room = self.GetParent<Room>();
            long timeNow = TimeHelper.ServerFrameTime();
            
            if (timeNow < room.FixedTimeCounter.FrameTime(room.AuthorityFrame + 1))
            {
                return;
            }

            ++room.AuthorityFrame;

            if (room.AuthorityFrame >= room.Replay.FrameInputs.Count)
            {
                return;
            }
            
            OneFrameInputs oneFrameInputs = room.Replay.FrameInputs[room.AuthorityFrame];
            
            room.Update(oneFrameInputs, room.AuthorityFrame);
        }
    }
}