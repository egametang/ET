namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class Room2C_EnterMapHandler: AMHandler<Room2C_EnterMap>
    {
        protected override async ETTask Run(Session session, Room2C_EnterMap message)
        {
            session.DomainScene().GetComponent<ObjectWait>().Notify(new WaitType.Wait_Room2C_EnterMap() {Message = message});
            await ETTask.CompletedTask;
        }
    }
}