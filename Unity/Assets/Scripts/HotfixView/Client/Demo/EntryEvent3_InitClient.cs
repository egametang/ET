using System;
using System.IO;

namespace ET.Client
{
    [Event(SceneType.Process)]
    public class EntryEvent3_InitClient: AEvent<Fiber, ET.EventType.EntryEvent3>
    {
        protected override async ETTask Run(Fiber scene, ET.EventType.EntryEvent3 args)
        {
            // 加载配置
            scene.AddComponent<ResourcesComponent>();

            await ResourcesComponent.Instance.LoadBundleAsync("unit.unity3d");

            SceneType sceneType = EnumHelper.FromString<SceneType>(GlobalComponent.Instance.GlobalConfig.AppType.ToString());
            
            Scene clientScene = await SceneFactory.CreateClientScene(scene, 1, sceneType, sceneType.ToString());
            
            await EventSystem.Instance.PublishAsync(clientScene, new EventType.AppStartInitFinish());
        }
    }
}