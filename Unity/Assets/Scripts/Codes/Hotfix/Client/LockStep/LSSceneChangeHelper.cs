namespace ET.Client
{

    public static class LSSceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene clientScene, string sceneName, long sceneInstanceId)
        {
            clientScene.RemoveComponent<BattleScene>();

            BattleScene battleScene = clientScene.AddComponent<BattleScene>();
            battleScene.Name = sceneName;

            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(clientScene, new EventType.LockStepSceneChangeStart());

            clientScene.GetComponent<SessionComponent>().Session.Send(new C2Room_ChangeSceneFinish());
            
            // 等待Room2C_EnterMap消息
            WaitType.Wait_Room2C_EnterMap waitRoom2CEnterMap = await clientScene.GetComponent<ObjectWait>().Wait<WaitType.Wait_Room2C_EnterMap>();

            battleScene.LSWorld = new LSWorld(SceneType.LockStepClient);
            battleScene.Init(waitRoom2CEnterMap.Message);
            
            battleScene.AddComponent<BattleSceneClientUpdater>();

            // 这个事件中可以订阅取消loading
            EventSystem.Instance.Publish(clientScene, new EventType.LockStepSceneInitFinish());
        }
    }
}