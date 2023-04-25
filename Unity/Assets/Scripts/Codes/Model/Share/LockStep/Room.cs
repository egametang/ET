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

        public FixedTimeCounter FixedTimeCounter { get; set; }
    }
}