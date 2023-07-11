namespace ET.Client
{
    [ActorMessageHandler(SceneType.All)]
    public class NetClient2Main_SessionDisposeHandler: ActorMessageHandler<Scene, NetClient2Main_SessionDispose>
    {
        protected override async ETTask Run(Scene entity, NetClient2Main_SessionDispose message)
        {
            Log.Error($"session dispose, error: {message.Error}");
            await ETTask.CompletedTask;
        }
    }
}