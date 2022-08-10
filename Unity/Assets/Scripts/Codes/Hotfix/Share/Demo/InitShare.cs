namespace ET
{
    [Callback(CallbackType.InitShare)]
    public class InitShare: IFunc<ETTask>
    {
        public async ETTask Handle()
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