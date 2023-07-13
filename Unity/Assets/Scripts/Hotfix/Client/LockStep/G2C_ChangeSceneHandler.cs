namespace ET.Client
{
    [ActorMessageHandler(SceneType.LockStep)]
    public class Match2G_NotifyMatchSuccessHandler: ActorMessageHandler<Scene, Match2G_NotifyMatchSuccess>
    {
        protected override async ETTask Run(Scene root, Match2G_NotifyMatchSuccess message)
        {
            await LSSceneChangeHelper.SceneChangeTo(root, "Map1", message.ActorId.InstanceId);
        }
    }
}