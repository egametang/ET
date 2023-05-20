namespace ET.Server
{
    [ActorMessageHandler(SceneType.Room)]
    public class C2Room_CheckHashHandler: ActorMessageHandler<Room, C2Room_CheckHash>
    {
        protected override async ETTask Run(Room room, C2Room_CheckHash message)
        {
            long hash = room.FrameBuffer.GetHash(message.Frame);
            if (message.Hash != hash)
            {
                byte[] bytes = room.FrameBuffer.Snapshot(message.Frame).ToArray();
                Room2C_CheckHashFail room2CCheckHashFail = new() { Frame = message.Frame, LSWorldBytes = bytes };
                ActorLocationSenderComponent.Instance.Get(LocationType.GateSession).Send(message.PlayerId, room2CCheckHashFail);
            }
            await ETTask.CompletedTask;
        }
    }
}