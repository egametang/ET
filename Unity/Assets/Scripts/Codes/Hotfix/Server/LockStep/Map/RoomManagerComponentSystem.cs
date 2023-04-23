using System;

namespace ET.Server
{

    public static class RoomManagerComponentSystem
    {
        public static async ETTask<Room> CreateBattleScene(this RoomManagerComponent self, Match2Map_GetRoom request)
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