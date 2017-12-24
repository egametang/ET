using System;
using Model;

namespace Hotfix
{
    [MessageHandler(AppType.DDZ)]
    public class CreateRoomRtHandler : AMRpcHandler<CreateRoomRt, CreateRoomRe>
    {
        protected override async void Run(Session session, CreateRoomRt message, Action<CreateRoomRe> reply)
        {
            CreateRoomRe response = new CreateRoomRe();
            try
            {
                //创建房间
                Room room = RoomFactory.Create(message.Level);
                await room.AddComponent<ActorComponent>().AddLocation();
                Log.Info($"新建房间{room.Id}");

                response.RoomId = room.Id;
                reply(response);
            }
            catch (Exception e)
            {
                ReplyError(response, e, reply);
            }
        }
    }
}
