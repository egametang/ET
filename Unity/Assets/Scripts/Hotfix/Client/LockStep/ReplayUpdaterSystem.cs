using System;

namespace ET.Client
{
    [FriendOf(typeof(ReplayUpdater))]
    public static class ReplayComponentSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<ReplayUpdater, Record>
        {
            protected override void Awake(ReplayUpdater self, Record record)
            {
                self.Record = record;
                self.GetParent<Room>().Init(self.Record.UnitInfos, TimeHelper.ServerFrameTime());
            }
        }

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
            OneFrameInputs oneFrameInputs = self.Record.FrameInputs[room.AuthorityFrame];
            room.Update(oneFrameInputs, room.AuthorityFrame);
        }
    }
}