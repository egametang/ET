using System;
using System.IO;

namespace ET
{
    [FriendOf(typeof(Room))]
    public static class RoomSystem
    {
        public static void Init(this Room self, Room2C_Start room2CStart)
        {
            self.StartTime = room2CStart.StartTime;
            
            self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);

            LSWorld lsWorld = self.GetComponent<LSWorld>();
            lsWorld.AddComponent<LSUnitComponent>();
            for (int i = 0; i < room2CStart.UnitInfo.Count; ++i)
            {
                LockStepUnitInfo unitInfo = room2CStart.UnitInfo[i];
                LSUnitFactory.Init(lsWorld, unitInfo);
                self.PlayerIds.Add(unitInfo.PlayerId);
            }
        }


        public static void Update(this Room self, OneFrameInputs oneFrameInputs, int frame)
        {
            LSWorld lsWorld = self.GetComponent<LSWorld>();
            // 保存当前帧场景数据
            self.FrameBuffer.SaveLSWorld(frame, lsWorld);

            if (frame <= self.RealFrame) // 只有Real帧才保存录像数据
            {
                self.SaveData(frame);
            }

            // 设置输入到每个LSUnit身上
            LSUnitComponent unitComponent = lsWorld.GetComponent<LSUnitComponent>();
            foreach (var kv in oneFrameInputs.Inputs)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                LSInputComponent lsInputComponent = lsUnit.GetComponent<LSInputComponent>();
                lsInputComponent.LSInput = kv.Value;
            }
            
            lsWorld.Update();
        }

        public static void SaveData(this Room self, int frame)
        {
            OneFrameInputs oneFrameInputs = self.FrameBuffer[frame];
            OneFrameInputs saveInput = new();
            oneFrameInputs.CopyTo(saveInput);
            self.SaveData.MessagesList.Add(saveInput);
            if (frame % LSConstValue.SaveLSWorldFrameCount == 0)
            {
                MemoryBuffer memoryBuffer = self.FrameBuffer.GetMemoryBuffer(frame);
                self.SaveData.LSWorlds.Add(memoryBuffer.ToArray());   
            }
        }
    }
}