namespace ET.Client
{
    public static class SceneChangeHelper
    {
        // 场景切换协程
        public static async ETTask SceneChangeTo(Scene clientScene, string sceneName, long sceneInstanceId)
        {
            clientScene.RemoveComponent<AIComponent>();
            
            CurrentScenesComponent currentScenesComponent = clientScene.GetComponent<CurrentScenesComponent>();
            currentScenesComponent.Scene?.Dispose(); // 删除之前的CurrentScene，创建新的
            Scene currentScene = SceneFactory.CreateCurrentScene(sceneInstanceId, clientScene.Zone, sceneName, currentScenesComponent);
            UnitComponent unitComponent = currentScene.AddComponent<UnitComponent>();
         
            // 可以订阅这个事件中创建Loading界面
            EventSystem.Instance.Publish(clientScene, new EventType.SceneChangeStart());

            // 等待CreateMyUnit的消息
            Wait_CreateMyUnit waitCreateMyUnit = await clientScene.GetComponent<ObjectWait>().Wait<Wait_CreateMyUnit>();
            M2C_CreateMyUnit m2CCreateMyUnit = waitCreateMyUnit.Message;
            Unit unit = UnitFactory.Create(currentScene, m2CCreateMyUnit.Unit);
            unitComponent.Add(unit);
            
            clientScene.RemoveComponent<AIComponent>();
            
            EventSystem.Instance.Publish(currentScene, new EventType.SceneChangeFinish());

            // 通知等待场景切换的协程
            clientScene.GetComponent<ObjectWait>().Notify(new Wait_SceneChangeFinish());
        }
    }
}