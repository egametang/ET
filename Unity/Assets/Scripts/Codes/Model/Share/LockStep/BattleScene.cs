using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class BattleScene: Entity, IScene, IAwake, IUpdate
    {
        public SceneType SceneType { get; set; } = SceneType.Battle;
        public string Name { get; set; }
        
        private long lsSceneInstanceId;
        
        public LSScene LSScene
        {
            get
            {
                return Root.Instance.Get(this.lsSceneInstanceId) as LSScene;
            }
            set
            {
                this.AddChild(value);
                this.lsSceneInstanceId = value.InstanceId;
            }
        }

        public int Frame;

        public long StartTime { get; set; }

        public FrameBuffer FrameBuffer { get; } = new();
    }
}