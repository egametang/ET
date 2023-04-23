namespace ET
{
    [FriendOf(typeof(Room))]
    public static class RoomSystem
    {
        public static void Init(this Room self, Room2C_Start room2CBattleStart)
        {
            self.StartTime = room2CBattleStart.StartTime;

            for (int i = 0; i < room2CBattleStart.UnitInfo.Count; ++i)
            {
                LockStepUnitInfo unitInfo = room2CBattleStart.UnitInfo[i];
                LSUnitFactory.Init(self.LSWorld, unitInfo);
            }
        }


        public static void Update(this Room self, OneFrameMessages oneFrameMessages)
        {
            // 保存当前帧场景数据
            self.FrameBuffer.SaveDate(self.FrameBuffer.NowFrame, MongoHelper.Serialize(self.LSWorld));
            
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
        }

        // 回滚
        public static void Rollback(this Room self, int frame)
        {
            Log.Debug($"Battle Scene roll back to {frame}");
            self.LSWorld.Dispose();
            byte[] dataBuffer = self.FrameBuffer.GetDate(frame);
            self.LSWorld = MongoHelper.Deserialize<LSWorld>(dataBuffer);
        }
    }
}