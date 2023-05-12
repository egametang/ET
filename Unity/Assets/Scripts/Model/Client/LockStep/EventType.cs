namespace ET
{
    namespace EventType
    {
        public struct LSSceneChangeStart
        {
            public Room Room;
        }
        
        public struct LSSceneInitFinish
        {
        }

        public struct LSAfterUnitCreate
        {
            public LSUnit LsUnit;
        }
    }
}