using System;

namespace ET.Server
{

    public static class RoomManagerComponentSystem
    {
        public static async ETTask<Scene> CreateRoom(this RoomManagerComponent self, Match2Map_GetRoom request)
        {
            long instanceId = IdGenerater.Instance.GenerateInstanceId();
            Scene scene = await SceneFactory.CreateServerScene(
                self, instanceId, instanceId, 
                self.DomainZone(), "Room", SceneType.Room);
            
            scene.AddComponent<RoomServerComponent, Match2Map_GetRoom>(request);
            
            scene.AddComponent<ServerFrameRecvComponent>();
            
            BattleScene battleScene = scene.AddComponent<BattleScene>();
            battleScene.LSWorld = new LSWorld(SceneType.LockStepClient);
            
            return scene;
        }
    }
}