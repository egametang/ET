using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace ET
{
    [FriendOf(typeof(BattleScene))]
    public static class BattleSceneSystem
    {
        [ObjectSystem]
        public class UpdateSystem : UpdateSystem<BattleScene>
        {
            protected override void Update(BattleScene self)
            {
                long timeNow = TimeHelper.ServerFrameTime();
                if (timeNow > self.StartTime + self.Frame * 50)
                {
                    OneFrameMessages oneFrameMessages = self.FrameBuffer.GetFrameMessage(self.Frame);
                    self.Update(oneFrameMessages);
                    ++self.Frame;
                }
            }
        }
        

        public static void Init(this BattleScene self, Room2C_EnterMap room2CEnterMap)
        {
            self.StartTime = room2CEnterMap.StartTime;
            
            foreach (LockStepUnitInfo lockStepUnitInfo in room2CEnterMap.UnitInfo)
            {
                UnitFFactory.Init(self.LSWorld, lockStepUnitInfo);
            }
        }

        public static void Update(this BattleScene self, OneFrameMessages oneFrameMessages)
        {
            // 保存当前帧场景数据
            self.FrameBuffer.SaveDate(self.Frame, MongoHelper.Serialize(self.LSWorld));
            
            
            // 处理Message
            
            
            self.LSWorld.Updater.Update();
        }

        // 回滚
        public static void Rollback(this BattleScene self, int frame)
        {
            byte[] dataBuffer = self.FrameBuffer.GetDate(frame);
            self.LSWorld.Dispose();
            LSWorld lsWorld = MongoHelper.Deserialize<LSWorld>(dataBuffer);
            self.LSWorld = lsWorld;
        }
    }
}