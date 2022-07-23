using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ET
{
    public class AppStart_Init : AEvent<EventType.AppStart>
    {
        protected override void Run(EventType.AppStart args)
        {
            RunAsync(args).Forget();
        }

        private async UniTaskVoid RunAsync(EventType.AppStart args)
        {
            await Tables.Ins.Init(new ConfigLoader());
            Game.Scene.AddComponent<TimerComponent>();
            Game.Scene.AddComponent<CoroutineLockComponent>();

            Game.Scene.AddComponent<OpcodeTypeComponent>();
            Game.Scene.AddComponent<MessageDispatcherComponent>();

            Game.Scene.AddComponent<NetThreadComponent>();
            Game.Scene.AddComponent<SessionStreamDispatcher>();
            Game.Scene.AddComponent<ZoneSceneManagerComponent>();

            Game.Scene.AddComponent<GlobalComponent>();
            Game.Scene.AddComponent<NumericWatcherComponent>();
            Game.Scene.AddComponent<AIDispatcherComponent>();

            Scene zoneScene = SceneFactory.CreateZoneScene(1, "Game", Game.Scene);
            Log.Info("加载成功");
            await UILoginPanel.CreateAsync();
        }
    }
}
