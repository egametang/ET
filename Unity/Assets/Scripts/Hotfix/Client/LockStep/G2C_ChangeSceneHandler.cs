namespace ET.Client
{
    public static partial class Match2G_NotifyMatchSuccessHandler
    {
        [MessageHandler(SceneType.LockStep)]
        private static async ETTask Run(Session session, Match2G_NotifyMatchSuccess message)
        {
            await LSSceneChangeHelper.SceneChangeTo(session.DomainScene(), "Map1", message.InstanceId);
        }
    }
}