using System;

namespace ET.Server
{

    public static class RoomManagerComponentSystem
    {
        public static async ETTask<Scene> CreateRoom(this RoomManagerComponent self, Match2Map_GetRoom match2MapGetRoom)
        {
            long instanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene scene = await SceneFactory.CreateServerScene(
                self, instanceId, instanceId, 
                self.DomainZone(), "Room", SceneType.Room);

            scene.AddComponent<RoomComponent, Match2Map_GetRoom>(match2MapGetRoom);
            
            return scene;
        }
    }
}