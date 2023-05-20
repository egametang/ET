namespace ET.Client
{
    public static partial class Room2C_CheckHashFailHandler
    {
        [MessageHandler(SceneType.LockStep)]
        private static async ETTask Run(Session session, Room2C_CheckHashFail message)
        {
            LSWorld serverWorld = MongoHelper.Deserialize(typeof(LSWorld), message.LSWorldBytes, 0, message.LSWorldBytes.Length) as LSWorld;
            using (session.ClientScene().AddChild(serverWorld))
            {
                Log.Debug($"check hash fail, server: {message.Frame} {serverWorld.ToJson()}");
            }

            Room room = session.ClientScene().GetComponent<Room>();
            LSWorld clientWorld = room.GetLSWorld(SceneType.LockStepClient, message.Frame);
            using (session.ClientScene().AddChild(clientWorld))
            {
                Log.Debug($"check hash fail, client: {message.Frame} {clientWorld.ToJson()}");
            }
            await ETTask.CompletedTask;
        }
    }
}