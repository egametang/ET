using System;
using System.Collections.Generic;

namespace ET
{
    [FriendOf(typeof(BattleScene))]
    public static class BattleSceneSystem
    {
        [ObjectSystem]
        public class AwakeSystem: AwakeSystem<BattleScene>
        {
            protected override void Awake(BattleScene self)
            {
                
            }
        }
        
        [ObjectSystem]
        public class UpdateSystem : UpdateSystem<BattleScene>
        {
            protected override void Update(BattleScene self)
            {
                long timeNow = TimeHelper.ServerFrameTime();
                if (timeNow > self.StartTime + self.Frame * 50)
                {
                    OneFrameMessage oneFrameMessage = self.FrameBuffer.GetFrameMessage(self.Frame);
                    self.Update(oneFrameMessage);
                    ++self.Frame;
                }
            }
        }
        

        public static void Init(this BattleScene self, Room2C_EnterMap room2CEnterMap)
        {
            self.StartTime = room2CEnterMap.StartTime;
            
            foreach (LockStepUnitInfo lockStepUnitInfo in room2CEnterMap.UnitInfo)
            {
                UnitFFactory.Init(self.LSScene, lockStepUnitInfo);
            }
        }

        public static void Update(this BattleScene self, OneFrameMessage oneFrameMessage)
        {
            // 保存当前帧场景数据
            self.FrameBuffer.SaveDate(self.Frame, MongoHelper.Serialize(self.LSScene));
            
            
            // 处理Message
            
            
            self.LSScene.Updater.Update();
        }
    }
}