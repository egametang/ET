namespace ET.Client
{
    public static partial class Room2C_EnterMapHandler
    {
        [MessageHandler(SceneType.LockStep)]
        private static async ETTask Run(Session session, Room2C_Start message)
        {
            session.DomainScene().GetComponent<ObjectWait>().Notify(new WaitType.Wait_Room2C_Start() {Message = message});
            await ETTask.CompletedTask;
        }
    }
}