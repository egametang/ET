using System;

namespace ET.Client
{
    [FriendOf(typeof(LSReplayUpdater))]
    public static class LSReplayComponentSystem
    {
        [EntitySystem]
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
            long timeNow = TimeHelper.ServerNow();

            int i = 0;
            while (true)
            {
                if (room.AuthorityFrame + 1 >= room.Replay.FrameInputs.Count)
                {
                    break;
                }
                
                if (timeNow < room.FixedTimeCounter.FrameTime(room.AuthorityFrame + 1))
                {
                    break;
                }

                ++room.AuthorityFrame;

                OneFrameInputs oneFrameInputs = room.Replay.FrameInputs[room.AuthorityFrame];
            
                room.Update(oneFrameInputs, room.AuthorityFrame);
                room.SpeedMultiply = ++i;
                
                long timeNow2 = TimeHelper.ServerNow();
                if (timeNow2 - timeNow > 5)
                {
                    break;
                }
            }
        }
    }
}