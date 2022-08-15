namespace ET
{
    [Callback(InitCallbackId.InitShare)]
    public class InitShare: ACallbackHandler<InitCallback, ETTask>
    {
        public override async ETTask Handle(InitCallback args)
        {
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();
            Game.Scene.AddComponent<NetThreadComponent>();
            Game.Scene.AddComponent<NumericWatcherComponent>();
            Game.Scene.AddComponent<AIDispatcherComponent>();
            Game.Scene.AddComponent<ClientSceneManagerComponent>();
            
            await Game.Scene.AddComponent<ConfigComponent>().LoadAsync();
        }
    }
}