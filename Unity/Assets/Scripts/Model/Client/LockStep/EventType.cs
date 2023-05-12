namespace ET
{
    namespace EventType
    {
        public struct LockStepSceneChangeStart
        {
            public Room Room;
        }
        
        public struct LockStepSceneInitFinish
        {
        }

        public struct LSAfterUnitCreate
        {
            public LSUnit LsUnit;
        }
    }
}