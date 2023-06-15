using System.Net.Sockets;

namespace ET.Client
{
    public static partial class SceneFactory
    {
        public static async ETTask<Scene> CreateClientScene(VProcess vProcess, int zone, SceneType sceneType, string name)
        {
            await ETTask.CompletedTask;
            
            Scene clientScene = EntitySceneFactory.CreateScene(vProcess, zone, sceneType, name, ClientSceneManagerComponent.Instance);
            clientScene.AddComponent<ObjectWait>();
            clientScene.AddComponent<PlayerComponent>();
            clientScene.AddComponent<CurrentScenesComponent>();
            
            EventSystem.Instance.Publish(clientScene, new EventType.AfterCreateClientScene());
            return clientScene;
        }
        
        public static Scene CreateCurrentScene(VProcess vProcess, long id, int zone, string name, CurrentScenesComponent currentScenesComponent)
        {
            Scene currentScene = EntitySceneFactory.CreateScene(vProcess, id, IdGenerater.Instance.GenerateInstanceId(), zone, SceneType.Current, name, currentScenesComponent);
            currentScenesComponent.Scene = currentScene;
            
            EventSystem.Instance.Publish(currentScene, new EventType.AfterCreateCurrentScene());
            return currentScene;
        }
    }
}