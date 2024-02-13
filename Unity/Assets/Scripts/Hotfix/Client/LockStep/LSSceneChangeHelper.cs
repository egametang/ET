namespace ET.Client
{

    public static partial class LSSceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene root, string sceneName, long sceneInstanceId)
        {
            root.RemoveComponent<Room>();

            Room room = root.AddComponentWithId<Room>(sceneInstanceId);
            room.Name = sceneName;

            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(root, new LSSceneChangeStart() {Room = room});

            root.GetComponent<ClientSenderComponent>().Send(C2Room_ChangeSceneFinish.Create());
            
            // 等待Room2C_EnterMap消息
            WaitType.Wait_Room2C_Start waitRoom2CStart = await root.GetComponent<ObjectWait>().Wait<WaitType.Wait_Room2C_Start>();

            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(waitRoom2CStart.Message.UnitInfo, waitRoom2CStart.Message.StartTime);
            
            room.AddComponent<LSClientUpdater>();

            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(root, new LSSceneInitFinish());
        }
        
        // 场景切换协程
        public static async ETTask SceneChangeToReplay(Scene root, Replay replay)
        {
            root.RemoveComponent<Room>();

            Room room = root.AddComponent<Room>();
            room.Name = "Map1";
            room.IsReplay = true;
            room.Replay = replay;
            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(replay.UnitInfos, TimeInfo.Instance.ServerFrameTime());
            
            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(root, new LSSceneChangeStart() {Room = room});
            

            room.AddComponent<LSReplayUpdater>();
            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(root, new LSSceneInitFinish());
        }
        
        // 场景切换协程
        public static async ETTask SceneChangeToReconnect(Scene root, G2C_Reconnect message)
        {
            root.RemoveComponent<Room>();

            Room room = root.AddComponent<Room>();
            room.Name = "Map1";
            
            room.LSWorld = new LSWorld(SceneType.LockStepClient);
            room.Init(message.UnitInfos, message.StartTime, message.Frame);
            
            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(root, new LSSceneChangeStart() {Room = room});


            room.AddComponent<LSClientUpdater>();
            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(root, new LSSceneInitFinish());
        }
    }
}