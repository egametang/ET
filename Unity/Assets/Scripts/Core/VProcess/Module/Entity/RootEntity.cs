using System.Collections.Generic;

namespace ET
{
    // 管理根部的Scene
    [EnableMethod]
    public class RootEntity: Entity, IScene, IVProcessSingleton
    {
        public RootEntity Root { get; set; }
        
        public VProcess VProcess { get; set; }

        public SceneType SceneType { get; set; }

        public RootEntity()
        {
            this.Root = this;
            this.Id = 0;
            this.InstanceId = IdGenerater.Instance.GenerateInstanceId();
            this.SceneType = SceneType.Root;
            this.IsCreated = true;
            this.IsNew = true;
            this.IsRegister = true;
            this.IScene = this;
            Log.Info($"Root create: {this.SceneType} {this.Id} {this.InstanceId}");
        }
    }
}