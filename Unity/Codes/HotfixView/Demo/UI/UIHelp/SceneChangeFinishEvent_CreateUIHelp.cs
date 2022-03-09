namespace ET
{
    public class SceneChangeFinishEvent_CreateUIHelp : AEvent<EventType.SceneChangeFinish>
    {
        protected override async ETTask Run(EventType.SceneChangeFinish arg)
        {
            await UIHelper.Create(arg.CurrentScene, UIType.UIHelp, UILayer.Mid);
        }
    }
}
