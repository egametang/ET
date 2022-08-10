using UnityEngine;

namespace ET
{
    namespace EventType
    {
        public struct ChangePosition
        {
            public Unit Unit;
            public Vector3 OldPos;
        }

        public struct ChangeRotation
        {
            public Unit Unit;
        }
    }
}