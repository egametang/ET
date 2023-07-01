using System.Collections.Generic;

namespace ET.Server
{
    [ActorMessageHandler(SceneType.RoomRoot)]
    public class RoomManager2Room_InitHandler: ActorMessageHandler<Scene, RoomManager2Room_Init, Room2RoomManager_Init>
    {
        protected override async ETTask Run(Scene root, RoomManager2Room_Init request, Room2RoomManager_Init response)
        {
            Room room = root.AddComponent<Room>();
            
            room.AddComponent<RoomServerComponent, List<long>>(request.PlayerIds);

            room.LSWorld = new LSWorld(SceneType.LockStepServer);
            await ETTask.CompletedTask;
        }
    }
}