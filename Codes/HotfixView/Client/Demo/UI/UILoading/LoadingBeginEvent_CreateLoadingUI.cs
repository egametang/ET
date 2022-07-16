using UnityEngine;

namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LoadingBeginEvent_CreateLoadingUI : AEvent<Scene, EventType.LoadingBegin>
    {
        protected override async ETTask Run(Scene scene, EventType.LoadingBegin args)
        {
            await UIHelper.Create(scene, UIType.UILoading, UILayer.Mid);
        }
    }
}
