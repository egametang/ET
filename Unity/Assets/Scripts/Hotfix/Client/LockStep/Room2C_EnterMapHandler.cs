namespace ET.Client
{
    [MessageHandler(SceneType.LockStep)]
    public class Room2C_EnterMapHandler: MessageHandler<Room2C_Start>
    {
        protected override async ETTask Run(Session session, Room2C_Start message)
        {
            session.DomainScene().GetComponent<ObjectWait>().Notify(new WaitType.Wait_Room2C_Start() {Message = message});
            await ETTask.CompletedTask;
        }
    }
}