namespace ET.Client
{
    [ActorMessageHandler(SceneType.LockStep)]
    public class G2C_ReconnectHandler: ActorMessageHandler<Scene, G2C_Reconnect>
    {
        protected override async ETTask Run(Scene root, G2C_Reconnect message)
        {
            await LSSceneChangeHelper.SceneChangeToReconnect(root, message);
            await ETTask.CompletedTask;
        }
    }
}