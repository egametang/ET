namespace ET
{
    public class SceneChangeFinishEventAsyncCreateUIHelp : AEvent<EventType.SceneChangeFinish>
    {
        protected override void Run(EventType.SceneChangeFinish args)
        {
            UIHelper.Create(args.CurrentScene, UIType.UIHelp, UILayer.Mid).Coroutine();
        }
    }
}
