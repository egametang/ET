using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ET
{
    [ComponentOf(typeof(Unit))]
    public class MoveComponent: Entity, IAwake, IDestroy
    {
        public float3 PreTarget
        {
            get
            {
                return this.Targets[this.N - 1];
            }
        }

        public float3 NextTarget
        {
            get
            {
                return this.Targets[this.N];
            }
        }

        // 开启移动协程的时间
        public long BeginTime;

        // 每个点的开始时间
        public long StartTime { get; set; }

        // 开启移动协程的Unit的位置
        public float3 StartPos;

        public float3 RealPos
        {
            get
            {
                return this.Targets[0];
            }
        }

        private long needTime;

        public long NeedTime
        {
            get
            {
                return this.needTime;
            }
            set
            {
                this.needTime = value;
            }
        }

        public long MoveTimer;

        public float Speed; // m/s

        public ETTask<bool> tcs;

        public List<float3> Targets = new List<float3>();

        public float3 FinalTarget
        {
            get
            {
                return this.Targets[this.Targets.Count - 1];
            }
        }

        public int N;

        public int TurnTime;

        public bool IsTurnHorizontal;

        public quaternion From;

        public quaternion To;
    }
}