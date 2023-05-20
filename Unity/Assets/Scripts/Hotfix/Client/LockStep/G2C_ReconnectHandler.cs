namespace ET.Client
{
    public static partial class G2C_ReconnectHandler
    {
        [MessageHandler(SceneType.LockStep)]
        private static async ETTask Run(Session session, G2C_Reconnect message)
        {
            await LSSceneChangeHelper.SceneChangeToReconnect(session.ClientScene(), message);
            await ETTask.CompletedTask;
        }
    }
}