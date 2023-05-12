using System;

namespace ET.Client
{
    [FriendOf(typeof(ReplayUpdater))]
    public static class ReplayComponentSystem
    {
        [ObjectSystem]
        public class UpdateSystem: UpdateSystem<ReplayUpdater>
        {
            protected override void Update(ReplayUpdater self)
            {
                self.Update();
            }
        }

        private static void Update(this ReplayUpdater self)
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