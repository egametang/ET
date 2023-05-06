using System;

namespace ET
{
    [FriendOf(typeof(Room))]
    public static class RoomSystem
    {
        public static void Init(this Room self, Room2C_Start room2CStart)
        {
            self.StartTime = room2CStart.StartTime;
            
            self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);

            self.LSWorld.AddComponent<LSUnitComponent>();
            for (int i = 0; i < room2CStart.UnitInfo.Count; ++i)
            {
                LockStepUnitInfo unitInfo = room2CStart.UnitInfo[i];
                LSUnitFactory.Init(self.LSWorld, unitInfo);
            }
        }


        public static void Update(this Room self, OneFrameMessages oneFrameMessages, int frame)
        {
            if (frame == self.FrameBuffer.RealFrame + 1)
            {
                // 保存当前帧场景数据
                self.FrameBuffer.SaveLSWorld(frame, self.LSWorld);
            }

            // 设置输入到每个LSUnit身上
            LSWorld lsWorld = self.LSWorld;
            LSUnitComponent unitComponent = lsWorld.GetComponent<LSUnitComponent>();
            foreach (var kv in oneFrameMessages.Inputs)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                LSInputComponent lsInputComponent = lsUnit.GetComponent<LSInputComponent>();
                lsInputComponent.LSInput = kv.Value;
            }
            
            lsWorld.Updater.Update();
        }
    }
}