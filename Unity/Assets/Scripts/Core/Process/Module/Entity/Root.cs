using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ET
{
    // 管理根部的Scene
    public class Root: VProcessSingleton<Root>, ISingletonAwake
    {
        public Scene Scene { get; private set; }

        public void Awake()
        {
            this.Scene = EntitySceneFactory.CreateScene(this.VProcess, 0, SceneType.Process, "Process");
        }

        public override void Dispose()
        {
            this.Scene.Dispose();
        }
    }
}