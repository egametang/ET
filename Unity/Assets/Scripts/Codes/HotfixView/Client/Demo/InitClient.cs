using System;
using System.IO;

namespace ET.Client
{
    [Callback(InitCallbackId.InitClient)]
    public class InitClient: ACallbackHandler<InitCallback, ETTask>
    {
        public override async ETTask Handle(InitCallback args)
        {
            // 加载配置
            Game.Scene.AddComponent<ResourcesComponent>();
            
            Game.Scene.AddComponent<GlobalComponent>();

            await ResourcesComponent.Instance.LoadBundleAsync("unit.unity3d");
            
            Scene clientScene = await SceneFactory.CreateClientScene(1, "Game", Game.Scene);
            
            await Game.EventSystem.PublishAsync(clientScene, new EventType.AppStartInitFinish());
        }
    }
}