namespace ET.Server
{
    [ActorMessageHandler(SceneType.RoomRoot)]
    public class C2Room_CheckHashHandler: ActorMessageHandler<Scene, C2Room_CheckHash>
    {
        protected override async ETTask Run(Scene root, C2Room_CheckHash message)
        {
            Room room = root.GetComponent<Room>();
            long hash = room.FrameBuffer.GetHash(message.Frame);
            if (message.Hash != hash)
            {
                byte[] bytes = room.FrameBuffer.Snapshot(message.Frame).ToArray();
                Room2C_CheckHashFail room2CCheckHashFail = new() { Frame = message.Frame, LSWorldBytes = bytes };
                room.Root().GetComponent<ActorLocationSenderComponent>().Get(LocationType.GateSession).Send(message.PlayerId, room2CCheckHashFail);
            }
            await ETTask.CompletedTask;
        }
    }
}