namespace ET.Client
{
    [Event(SceneType.Client)]
    public class LoadingFinishEvent_RemoveLoadingUI : AEvent<Scene, EventType.LoadingFinish>
    {
        protected override async ETTask Run(Scene scene, EventType.LoadingFinish args)
        {
            await UIHelper.Create(scene, UIType.UILoading, UILayer.Mid);
        }
    }
}
