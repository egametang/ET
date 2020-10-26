

namespace ET
{
    public class AppStart_Init: AEvent<EventType.AppStart>
    {
        public override async ETTask Run(EventType.AppStart args)
        {
            Game.Scene.AddComponent<ConfigComponent>();

            Options options = Game.Scene.GetComponent<Options>();
            StartProcessConfig processConfig = StartProcessConfigCategory.Instance.Get(options.Process);
            
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();
            // 发送普通actor消息
            Game.Scene.AddComponent<ActorMessageSenderComponent>();
            // 发送location actor消息
            Game.Scene.AddComponent<ActorLocationSenderComponent>();
            // 访问location server的组件
            Game.Scene.AddComponent<LocationProxyComponent>();
            Game.Scene.AddComponent<ActorMessageDispatcherComponent>();
            // 数值订阅组件
            Game.Scene.AddComponent<NumericWatcherComponent>();
            // 控制台组件
            Game.Scene.AddComponent<ConsoleComponent>();
				
            Game.Scene.AddComponent<NetInnerComponent, string>(processConfig.InnerAddress);
            
            var processScenes = StartSceneConfigCategory.Instance.GetByProcess(IdGenerater.Process);
            foreach (StartSceneConfig startConfig in processScenes)
            {
                await SceneFactory.Create(Game.Scene, startConfig.SceneId, startConfig.Zone, startConfig.Name, startConfig.Type, startConfig);
            }
        }
    }
}