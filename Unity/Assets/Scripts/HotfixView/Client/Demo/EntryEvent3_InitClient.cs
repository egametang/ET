using System;
using System.IO;

namespace ET.Client
{
    [Event(SceneType.Main)]
    public class EntryEvent3_InitClient: AEvent<Scene, ET.EventType.EntryEvent3>
    {
        protected override async ETTask Run(Scene root, ET.EventType.EntryEvent3 args)
        {
            World.Instance.AddSingleton<UIEventComponent>();
            
            // 加载配置
            ResourcesComponent resourcesComponent = root.AddComponent<ResourcesComponent>();

            await resourcesComponent.LoadBundleAsync("unit.unity3d");

            GlobalComponent globalComponent = root.GetComponent<GlobalComponent>();

            SceneType sceneType = EnumHelper.FromString<SceneType>(globalComponent.GlobalConfig.AppType.ToString());

            // 根据配置修改掉Main Fiber的SceneType
            root.SceneType = sceneType;
            
            await EventSystem.Instance.PublishAsync(root, new EventType.AppStartInitFinish());
        }
    }
}