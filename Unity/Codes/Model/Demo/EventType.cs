using UnityEngine;

namespace ET
{
    namespace EventType
    {
        public class AppStart
        {
        }

        public class SceneChangeStart
        {
            public Scene ZoneScene;
        }
        
        
        public class SceneChangeFinish
        {
            public Scene ZoneScene;
            public Scene CurrentScene;
        }

        public class ChangePosition
        {
            public Unit Unit;
            public Vector3 OldPos;
        }

        public class ChangeRotation
        {
            public Unit Unit;
        }

        public class PingChange
        {
            public Scene ZoneScene;
            public long Ping;
        }
        
        public class AfterCreateZoneScene
        {
            public Scene ZoneScene;
        }
        
        public class AfterCreateCurrentScene
        {
            public Scene CurrentScene;
        }
        
        public class AfterCreateLoginScene
        {
            public Scene LoginScene;
        }

        public class AppStartInitFinish
        {
            public Scene ZoneScene;
        }

        public class LoginFinish
        {
            public Scene ZoneScene;
        }

        public class LoadingBegin
        {
            public Scene Scene;
        }

        public class LoadingFinish
        {
            public Scene Scene;
        }

        public class EnterMapFinish
        {
            public Scene ZoneScene;
        }

        public class AfterUnitCreate
        {
            public Unit Unit;
        }
        
        public class MoveStart
        {
            public Unit Unit;
        }

        public class MoveStop
        {
            public Unit Unit;
        }

        public class UnitEnterSightRange
        {
        }
    }
}