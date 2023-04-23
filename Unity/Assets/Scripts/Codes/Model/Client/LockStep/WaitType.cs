namespace ET
{
    namespace WaitType
    {
        public struct Wait_Room2C_EnterMap: IWaitType
        {
            public int Error { get; set; }

            public Battle2C_BattleStart Message;
        }
    }
}