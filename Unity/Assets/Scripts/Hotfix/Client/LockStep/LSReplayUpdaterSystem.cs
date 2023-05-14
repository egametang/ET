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
            long timeNow = TimeHelper.ServerFrameTime();

            int old = room.AuthorityFrame;
            for (int i = 0; i < 10; ++i)
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
                room.SpeedMultiply = i + 1;
            }

            if (room.AuthorityFrame > old)
            {
                Log.Debug($"111111111111111111 replay update: {old} {room.AuthorityFrame}");
            }
        }
    }
}