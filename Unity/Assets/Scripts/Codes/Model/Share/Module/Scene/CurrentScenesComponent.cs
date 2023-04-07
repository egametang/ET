namespace ET
{
    // 可以用来管理多个客户端场景，比如大世界会加载多块场景
    [ComponentOf(typeof(Scene))]
    public class CurrentScenesComponent: Entity, IAwake
    {
        private long sceneInstanceId;

        public Scene Scene
        {
            get
            {
                return Root.Instance.Get(this.sceneInstanceId) as Scene;
            }
            set
            {
                this.sceneInstanceId = value.InstanceId;
            }
        }
    }
}