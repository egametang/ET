namespace ET
{
    [ComponentOf(typeof(Scene))]
    public class BattleComponent: Entity, IAwake
    {
        private long sceneInstanceId;
        
        public LSScene LSScene
        {
            get
            {
                return Root.Instance.Get(this.sceneInstanceId) as LSScene;
            }
            set
            {
                this.AddChild(value);
                this.sceneInstanceId = value.InstanceId;
            }
        }
    }
}