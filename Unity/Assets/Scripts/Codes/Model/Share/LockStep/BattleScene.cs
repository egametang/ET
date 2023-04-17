namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class BattleScene: Entity, IScene, IAwake
    {
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

        public SceneType SceneType { get; set; } = SceneType.Battle;
        
        public string Name { get; set; }
    }
}