using System.Collections.Generic;
using TrueSync;

namespace ET
{
    [ChildOf]
    [ComponentOf]
    public class Room: Entity, IScene, IAwake, IUpdate
    {
        public SceneType SceneType { get; set; } = SceneType.Room;
        public string Name { get; set; }
        
        public long StartTime { get; set; }

        public FrameBuffer FrameBuffer { get; } = new();

        public FixedTimeCounter FixedTimeCounter { get; set; }

        public List<long> PlayerIds { get; } = new(LSConstValue.MatchCount);
        
        public int PredictionFrame { get; set; } = -1;

        public int RealFrame { get; set; } = -1;
    }
}