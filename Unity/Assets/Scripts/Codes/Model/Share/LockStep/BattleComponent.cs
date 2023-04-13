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
                value.Parent = this;
                this.sceneInstanceId = value.InstanceId;
            }
        }
    }
}