namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class FrameMessageHandler: AMActorHandler<Scene, FrameMessage>
    {
        protected override async ETTask Run(Scene scene, FrameMessage message)
        {
            BattleScene battleScene = scene.GetComponent<BattleScene>();

            await ETTask.CompletedTask;
        }
    }
}