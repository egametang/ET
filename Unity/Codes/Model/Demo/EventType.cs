using UnityEngine;

namespace ET
{
    namespace EventType
    {
        public struct AppStart
        {
        }

        public struct ChangePosition
        {
            public Unit Unit;
            public Vector3 OldPos;
        }

        public struct ChangeRotation
        {
            public Unit Unit;
        }

        public struct PingChange
        {
            public Scene ZoneScene;
            public long Ping;
        }
        
        public struct AfterCreateZoneScene
        {
            public Scene ZoneScene;
        }
        
        public struct AfterCreateLoginScene
        {
            public Scene LoginScene;
        }

        public struct AppStartInitFinish
        {
            public Scene ZoneScene;
        }

        public struct LoginFinish
        {
            public Scene ZoneScene;
        }

        public struct LoadingBegin
        {
            public Scene Scene;
        }

        public struct LoadingFinish
        {
            public Scene Scene;
        }

        public struct EnterMapFinish
        {
            public Scene ZoneScene;
        }

        public struct AfterUnitCreate
        {
            public Unit Unit;
        }
        
        public struct MoveStart
        {
            public Unit Unit;
        }

        public struct MoveStop
        {
            public Unit Unit;
        }

        public struct UnitEnterSightRange
        {
        }
    }
}