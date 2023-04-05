using System;

namespace ET.Server
{

    public static class RoomManagerComponentSystem
    {
        public static async ETTask<Scene> CreateRoom(this RoomManagerComponent self)
        {
            long instanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene room = await SceneFactory.CreateServerScene(
                self, instanceId, instanceId, 
                self.DomainZone(), "Room", SceneType.Room);
            return room;
        }
    }
}