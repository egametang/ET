namespace ET.Client
{
    [MessageHandler(SceneType.LockStep)]
    public class Room2C_CheckHashFailHandler: AMHandler<Room2C_CheckHashFail>
    {
        protected override async ETTask Run(Session session, Room2C_CheckHashFail message)
        {
            LSWorld serverWorld = MongoHelper.Deserialize(typeof(LSWorld), message.LSWorldBytes, 0, message.LSWorldBytes.Length) as LSWorld;
            using (session.ClientScene().AddChild(serverWorld))
            {
                Log.Debug($"check hash fail, server: {message.Frame} {serverWorld.ToJson()}");
            }

            LSWorld clientWorld = session.ClientScene().GetComponent<Room>().GetLSWorld(SceneType.LockStepClient, message.Frame);
            using (session.ClientScene().AddChild(clientWorld))
            {
                Log.Debug($"check hash fail, client: {message.Frame} {clientWorld.ToJson()}");
            }
            await ETTask.CompletedTask;
        }
    }
}