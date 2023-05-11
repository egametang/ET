using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [FriendOf(typeof(Room))]
    public static class RoomSystem
    {
        public static void Init(this Room self, List<LockStepUnitInfo> unitInfos, long startTime)
        {
            self.StartTime = startTime;
            
            self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);

            LSWorld lsWorld = self.LSWorld;
            lsWorld.AddComponent<LSUnitComponent>();
            for (int i = 0; i < unitInfos.Count; ++i)
            {
                LockStepUnitInfo unitInfo = unitInfos[i];
                LSUnitFactory.Init(lsWorld, unitInfo);
                self.PlayerIds.Add(unitInfo.PlayerId);
            }
        }


        public static void Update(this Room self, OneFrameInputs oneFrameInputs, int frame)
        {
            LSWorld lsWorld = self.LSWorld;

            if (!self.IsReplay)
            {
                // 保存当前帧场景数据
                self.SaveLSWorld(frame);

                if (frame <= self.AuthorityFrame) // 只有AuthorityFrame帧才保存录像数据
                {
                    self.Record(frame);
                }
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
        
        public static LSWorld GetLSWorld(this Room self, int frame)
        {
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            return MongoHelper.Deserialize(typeof (LSWorld), memoryBuffer) as LSWorld;
        }

        public static void SaveLSWorld(this Room self, int frame)
        {
            MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);
            
            MongoHelper.Serialize(self.LSWorld, memoryBuffer);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
        }

        public static void Record(this Room self, int frame)
        {
            OneFrameInputs oneFrameInputs = self.FrameBuffer.FrameInputs(frame);
            OneFrameInputs saveInput = new();
            oneFrameInputs.CopyTo(saveInput);
            self.Record.FrameInputs.Add(saveInput);
            if (frame % LSConstValue.SaveLSWorldFrameCount == 0)
            {
                MemoryBuffer memoryBuffer = self.FrameBuffer.Snapshot(frame);
                self.Record.Snapshots.Add(memoryBuffer.ToArray());   
            }
        }
    }
}