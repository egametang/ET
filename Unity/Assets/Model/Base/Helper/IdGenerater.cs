namespace ETModel
{
    public static class IdGenerater
    {
        public const int HeadPos = 50;
		
        private static long appId;

        public static long Head { get; private set; }
		
        public static long AppId
        {
            set
            {
                appId = value;
                Head = value << HeadPos;
            }
            get
            {
                return appId;
            }
        }

        public static long HeadMask = 0x0003ffffffffffff;

        private static ushort value;

        private static int sceneId = 100000;
		
        public static long GenerateSceneId()
        {
            return ++sceneId;
        }
		
        public static long GenerateSceneInstanceId(long id)
        {
            return IdGenerater.Head + id;
        }

        public static long GenerateId()
        {
            long time = TimeHelper.ClientNowSeconds();

            return Head + (time << 18) + ++value;
        }

        public static int GetProcessId(long v)
        {
            return (int)(v >> HeadPos);
        }
    }
}