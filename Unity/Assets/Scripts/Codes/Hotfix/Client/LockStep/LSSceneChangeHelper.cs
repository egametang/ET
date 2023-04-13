namespace ET.Client
{

    public static class LSSceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene clientScene, string sceneName, long sceneInstanceId)
        {
            clientScene.RemoveComponent<AIComponent>();

            CurrentScenesComponent currentScenesComponent = clientScene.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的

            Scene currentScene = LSSceneFactory.CreateClientRoomScene(sceneInstanceId, clientScene.Zone, sceneName, currentScenesComponent);

            // 等待表现层订阅的事件完成
            await EventSystem.Instance.PublishAsync(clientScene, new EventType.LockStepSceneChangeStart());

            clientScene.GetComponent<SessionComponent>().Session.Send(new C2Room_ChangeSceneFinish());
            
            // 等待Room2C_EnterMap消息
            WaitType.Wait_Room2C_EnterMap waitRoom2CEnterMap = await clientScene.GetComponent<ObjectWait>().Wait<WaitType.Wait_Room2C_EnterMap>();

            currentScene.GetComponent<BattleComponent>().InitUnit(waitRoom2CEnterMap.Message.UnitInfo);

            EventSystem.Instance.Publish(currentScene, new EventType.LockStepSceneInitFinish());
        }
    }
}