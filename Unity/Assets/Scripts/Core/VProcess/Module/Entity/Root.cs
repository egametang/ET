using System.Collections.Generic;

namespace ET
{
    // 管理根部的Scene
    [EnableMethod]
    public class Root: VProcessSingleton<Root>
    {
        private readonly Dictionary<long, Entity> mailboxEntities = new();
        
        public Scene Scene { get; private set; }

        public Root()
        {
            this.Scene = EntitySceneFactory.CreateScene(IdGenerater.Instance.GenerateId(), IdGenerater.Instance.GenerateInstanceId(), 0, SceneType.Root, "Root");
        }

        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();

            Scene scene = this.Scene;
            this.Scene = null;
            scene.Dispose();
        }
    }
}