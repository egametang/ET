namespace ET.Client
{

    [MessageHandler(SceneType.Client)]
    public class Match2G_NotifyMatchSuccessHandler: AMHandler<Match2G_NotifyMatchSuccess>
    {
        protected override async ETTask Run(Session session, Match2G_NotifyMatchSuccess message)
        {
            await LSSceneChangeHelper.SceneChangeTo(session.DomainScene(), "Map1", message.InstanceId);
        }
    }
}