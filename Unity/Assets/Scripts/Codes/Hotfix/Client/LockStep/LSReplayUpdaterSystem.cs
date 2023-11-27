using System;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSReplayUpdater))]
    [FriendOf(typeof(LSReplayUpdater))]
    public static partial class LSReplayUpdaterSystem
    {
        [EntitySystem]
        private static void Awake(this LSReplayUpdater self)
        {

        }
        
        [EntitySystem]
        private static void Update(this LSReplayUpdater self)
        {
            Room room = self.GetParent<Room>();
            Fiber fiber = self.Fiber();
            long timeNow = TimeInfo.Instance.ServerNow();

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

                room.Update(oneFrameInputs);
                room.SpeedMultiply = ++i;

                long timeNow2 = TimeInfo.Instance.ServerNow();
                if (timeNow2 - timeNow > 5)
                {
                    break;
                }
            }
        }

        public static void ChangeReplaySpeed(this LSReplayUpdater self)
        {
            Room room = self.Room();
            LSReplayUpdater lsReplayUpdater = room.GetComponent<LSReplayUpdater>();
            if (lsReplayUpdater.ReplaySpeed == 8)
            {
                lsReplayUpdater.ReplaySpeed = 1;
            }
            else
            {
                lsReplayUpdater.ReplaySpeed *= 2;
            }

            int updateInterval = LSConstValue.UpdateInterval / lsReplayUpdater.ReplaySpeed;
            room.FixedTimeCounter.ChangeInterval(updateInterval, room.AuthorityFrame);
        }
    }
}