using System;
using System.Threading;
using UnityEngine;

namespace ET
{
    public class MoveComponent: Entity
    {
        public Vector3 Target;

        // 开启移动协程的时间
        public long StartTime;

        // 开启移动协程的Unit的位置
        public Vector3 StartPos;

        public long needTime;

        public ETTaskCompletionSource moveTcs;
    }
}