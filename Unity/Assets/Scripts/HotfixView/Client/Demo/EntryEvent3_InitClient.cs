using System;
using System.IO;

namespace ET.Client
{
    [Event(SceneType.Main)]
    public class EntryEvent3_InitClient: AEvent<Scene, EventType.EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EventType.EntryEvent3 args)
        {
            World.Instance.AddSingleton<UIEventComponent>();
            
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            
            GlobalComponent globalComponent = root.AddComponent<GlobalComponent>();
            root.AddComponent<UIGlobalComponent>();
            root.AddComponent<UIComponent>();
            ResourcesComponent resourcesComponent = root.AddComponent<ResourcesComponent>();
            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            
            await resourcesComponent.LoadBundleAsync("unit.unity3d");
            
            // 根据配置修改掉Main Fiber的SceneType
            SceneType sceneType = EnumHelper.FromString<SceneType>(globalComponent.GlobalConfig.AppType.ToString());
            root.SceneType = sceneType;
            
            await EventSystem.Instance.PublishAsync(root, new EventType.AppStartInitFinish());
        }
    }
}