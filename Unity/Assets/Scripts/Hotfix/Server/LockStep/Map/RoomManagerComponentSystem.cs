using System;

namespace ET.Server
{

    public static class RoomManagerComponentSystem
    {
        public static async ETTask<Room> CreateServerRoom(this RoomManagerComponent self, Match2Map_GetRoom request)
        {
            await ETTask.CompletedTask;
            
            Room room = self.AddChild<Room>();
            
            room.AddComponent<RoomServerComponent, Match2Map_GetRoom>(request);

            LSWorld lsWorld = new(SceneType.LockStepServer);
            room.AddComponent(lsWorld);

            room.AddComponent<MailBoxComponent, MailboxType>(MailboxType.UnOrderMessageDispatcher);
            
            return room;
        }
    }
}