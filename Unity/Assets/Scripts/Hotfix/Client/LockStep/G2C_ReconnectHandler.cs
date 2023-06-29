namespace ET.Client
{
    [MessageHandler(SceneType.LockStep)]
    public class G2C_ReconnectHandler: MessageHandler<G2C_Reconnect>
    {
        protected override async ETTask Run(Session session, G2C_Reconnect message)
        {
            await LSSceneChangeHelper.SceneChangeToReconnect(session.Root(), message);
            await ETTask.CompletedTask;
        }
    }
}