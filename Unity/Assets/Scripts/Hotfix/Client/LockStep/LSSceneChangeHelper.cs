namespace ET.Client
{

    public static partial class LSSceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Fiber fiber, string sceneName, long sceneInstanceId)
        {
            fiber.RemoveComponent<Room>();

            Room room = fiber.AddComponentWithId<Room>(sceneInstanceId);
            room.Name = sceneName;

            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(fiber, new EventType.LSSceneChangeStart() {Room = room});

            fiber.GetComponent<SessionComponent>().Session.Send(new C2Room_ChangeSceneFinish());
            
            // 等待Room2C_EnterMap消息
            WaitType.Wait_Room2C_Start waitRoom2CStart = await fiber.GetComponent<ObjectWait>().Wait<WaitType.Wait_Room2C_Start>();

            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(waitRoom2CStart.Message.UnitInfo, waitRoom2CStart.Message.StartTime);
            
            room.AddComponent<LSClientUpdater>();

            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(fiber, new EventType.LSSceneInitFinish());
        }
        
        // 场景切换协程
        public static async ETTask SceneChangeToReplay(Fiber fiber, Replay replay)
        {
            fiber.RemoveComponent<Room>();

            Room room = fiber.AddComponent<Room>();
            room.Name = "Map1";
            room.IsReplay = true;
            room.Replay = replay;
            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(replay.UnitInfos, TimeHelper.ServerFrameTime());
            
            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(fiber, new EventType.LSSceneChangeStart() {Room = room});
            

            room.AddComponent<LSReplayUpdater>();
            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(fiber, new EventType.LSSceneInitFinish());
        }
        
        // 场景切换协程
        public static async ETTask SceneChangeToReconnect(Fiber fiber, G2C_Reconnect message)
        {
            fiber.RemoveComponent<Room>();

            Room room = fiber.AddComponent<Room>();
            room.Name = "Map1";
            
            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(message.UnitInfos, message.StartTime, message.Frame);
            
            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(fiber, new EventType.LSSceneChangeStart() {Room = room});


            room.AddComponent<LSClientUpdater>();
            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(fiber, new EventType.LSSceneInitFinish());
        }
    }
}