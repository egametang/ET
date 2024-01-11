using System;
using System.Collections.Generic;
using System.IO;
using YIUIFramework;

namespace ET.Client
{
    [Event(SceneType.Main)]
    public class EntryEvent3_InitClient: AEvent<Scene, EntryEvent3>
    {
        protected override async ETTask Run(Scene root, EntryEvent3 args)
        {
            GlobalComponent globalComponent = root.AddComponent<GlobalComponent>();
            root.AddComponent<UIGlobalComponent>();
            root.AddComponent<UIComponent>();
            root.AddComponent<ResourcesLoaderComponent>();
            root.AddComponent<PlayerComponent>();
            root.AddComponent<CurrentScenesComponent>();
            
            // 根据配置修改掉Main Fiber的SceneType
            SceneType sceneType = EnumHelper.FromString<SceneType>(globalComponent.GlobalConfig.AppType.ToString());
            root.SceneType = sceneType;

            YIUIBindHelper.InternalGameGetUIBindVoFunc = YIUICodeGenerated.YIUIBindProvider.Get;
            await root.AddComponent<YIUIMgrComponent>().Initialize();
            #region 根据需求自行处理
            //在editor下自动打开  也可以根据各种外围配置 或者 GM等级打开
#if UNITY_EDITOR
            root.AddComponent<GMCommandComponent>();
#endif
            #endregion


            await EventSystem.Instance.PublishAsync(root, new AppStartInitFinish());
        }
    }
}