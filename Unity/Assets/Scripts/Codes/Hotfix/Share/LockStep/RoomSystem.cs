namespace ET
{
    [FriendOf(typeof(Room))]
    public static class RoomSystem
    {
        
        
        public static void Init(this Room self, Room2C_Start room2CStart)
        {
            self.StartTime = room2CStart.StartTime;
            
            self.FixedTimeCounter = new FixedTimeCounter(self.StartTime, 0, LSConstValue.UpdateInterval);

            for (int i = 0; i < room2CStart.UnitInfo.Count; ++i)
            {
                LockStepUnitInfo unitInfo = room2CStart.UnitInfo[i];
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
            foreach (var kv in oneFrameMessages.Inputs)
            {
                LSUnit lsUnit = unitComponent.GetChild<LSUnit>(kv.Key);
                LSInput lsInput = lsUnit.GetComponent<LSInputComponent>().LSInput;
                lsInput.V = kv.Value.V;
                lsInput.Button = kv.Value.Button;
            }
            
            lsWorld.Updater.Update();
        }
    }
}