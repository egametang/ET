namespace ET
{
    [FriendOf(typeof(BattleScene))]
    public static class BattleSceneSystem
    {
        public static void Init(this BattleScene self, Room2C_BattleStart room2CBattleStart)
        {
            self.StartTime = room2CBattleStart.StartTime;

            for (int i = 0; i < room2CBattleStart.UnitInfo.Count; ++i)
            {
                LockStepUnitInfo unitInfo = room2CBattleStart.UnitInfo[i];
                UnitFFactory.Init(self.LSWorld, unitInfo);
            }
        }


        public static void Update(this BattleScene self, OneFrameMessages oneFrameMessages)
        {
            // 设置输入到每个LSUnit身上
            LSWorld lsWorld = self.LSWorld;
            LSUnitComponent unitComponent = lsWorld.GetComponent<LSUnitComponent>();
            foreach (var kv in oneFrameMessages.InputInfos)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                LSInputInfo lsInputInfo = lsUnit.GetComponent<LSUnitInputComponent>().LSInputInfo;
                lsInputInfo.V = kv.Value.V;
                lsInputInfo.Button = kv.Value.Button;
            }
            
            lsWorld.Updater.Update();
            
            // 保存当前帧场景数据
            self.FrameBuffer.SaveDate(self.FrameBuffer.NowFrame, MongoHelper.Serialize(self.LSWorld));
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