using System;

namespace ET.Server
{

    public static class BattleSceneManagerComponentSystem
    {
        public static async ETTask<Room> CreateBattleScene(this BattleSceneManagerComponent self, Match2Map_GetRoom request)
        {
            await ETTask.CompletedTask;
            
            Room room = self.AddChild<Room>();
            
            room.AddComponent<RoomServerComponent, Match2Map_GetRoom>(request);
            
            room.AddComponent<ServerFrameRecvComponent>();

            room.LSWorld = new LSWorld(SceneType.LockStepServer);

            room.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher);
            
            return room;
        }
    }
}