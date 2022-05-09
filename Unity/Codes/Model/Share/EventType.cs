using UnityEngine;

namespace ET
{
    namespace EventType
    {
        public struct AppStart
        {
        }
		
        public struct MoveStart
        {
        }

        public struct MoveStop
        {
        }
        
        public struct SceneChangeStart
        {
        }
        
        
        public struct SceneChangeFinish
        {
        }

        public struct ChangePosition
        {
            public Vector3 OldPos;
        }

        public struct ChangeRotation
        {
        }

        public struct PingChange
        {
            public long Ping;
        }
        
        public struct AfterCreateZoneScene
        {
        }
        
        public struct AfterCreateCurrentScene
        {
        }
        
        public struct AfterCreateLoginScene
        {
        }

        public struct AppStartInitFinish
        {
        }

        public struct LoginFinish
        {
        }

        public struct LoadingBegin
        {
        }

        public struct LoadingFinish
        {
        }

        public struct EnterMapFinish
        {
        }

        public struct AfterUnitCreate
        {
        }
    }
}