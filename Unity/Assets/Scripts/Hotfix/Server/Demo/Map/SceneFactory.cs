using System.Net;
using System.Net.Sockets;

namespace ET.Server
{
    public static partial class SceneFactory
    {
        public static async ETTask<Scene> CreateMapScene(Entity parent, long id, long instanceId, SceneType sceneType, string name, StartSceneConfig startSceneConfig = null)
        {
            await ETTask.CompletedTask;
            Scene scene = EntitySceneFactory.CreateScene(parent, id, instanceId, sceneType, name);

            scene.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);

            switch (scene.SceneType)
            {
                case SceneType.Map:
                    scene.AddComponent<UnitComponent>();
                    scene.AddComponent<AOIManagerComponent>();
                    scene.AddComponent<RoomManagerComponent>();
                    break;
            }

            return scene;
        }
        
    }
}