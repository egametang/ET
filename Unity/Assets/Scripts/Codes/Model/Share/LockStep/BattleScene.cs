using System.Collections.Generic;
using TrueSync;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class BattleScene: Entity, IScene, IAwake, IUpdate
    {
        public readonly struct SlotIdPair
        {
            private readonly long[] SlotIds = new long[LSConstValue.MatchCount];
            private readonly Dictionary<long, int> IdSlots = new(LSConstValue.MatchCount);

            public SlotIdPair()
            {
            }

            public long GetIdBySlot(int slot)
            {
                return this.SlotIds[slot];
            }
            
            public int GetSlotById(long id)
            {
                return this.IdSlots[id];
            }

            public void Add(int slot, long id)
            {
                this.SlotIds[slot] = id;
                this.IdSlots[id] = slot;
            }
        }
        
        public SceneType SceneType { get; set; } = SceneType.Battle;
        public string Name { get; set; }
        
        private long lsWorldInstanceId;
        
        public LSWorld LSWorld
        {
            get
            {
                return Root.Instance.Get(this.lsWorldInstanceId) as LSWorld;
            }
            set
            {
                this.AddChild(value);
                this.lsWorldInstanceId = value.InstanceId;
            }
        }

        public long StartTime { get; set; }

        public FrameBuffer FrameBuffer { get; } = new();

        public SlotIdPair SlotIds { get; } = new();
    }
}