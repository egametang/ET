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
        
        private EntityRef<LSWorld> lsWorld;
        
        public LSWorld LSWorld
        {
            get
            {
                return this.lsWorld;
            }
            set
            {
                this.AddChild(value);
                this.lsWorld = value;
            }
        }

        public long StartTime { get; set; }

        public FrameBuffer FrameBuffer { get; } = new();

        public FixedTimeCounter FixedTimeCounter { get; set; }
    }
}