using System;
using System.IO;

namespace ET.Client
{
    [Callback(CallbackType.InitClient)]
    public class InitClient: IFunc<ETTask>
    {
        public async ETTask Handle()
        {
            // 加载配置
            Game.Scene.AddComponent<ResourcesComponent>();
            
            Game.Scene.AddComponent<GlobalComponent>();

            await ResourcesComponent.Instance.LoadBundleAsync("unit.unity3d");
            
            Scene clientScene = SceneFactory.CreateClientScene(1, "Game", Game.Scene);
            
            await Game.EventSystem.PublishAsync(clientScene, new EventType.AppStartInitFinish());
        }
    }
}