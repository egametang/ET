namespace ET.Client
{
    [MessageHandler(SceneType.Client)]
    public class Room2C_EnterMapHandler: AMHandler<Battle2C_BattleStart>
    {
        protected override async ETTask Run(Session session, Battle2C_BattleStart message)
        {
            session.DomainScene().GetComponent<ObjectWait>().Notify(new WaitType.Wait_Room2C_EnterMap() {Message = message});
            await ETTask.CompletedTask;
        }
    }
}