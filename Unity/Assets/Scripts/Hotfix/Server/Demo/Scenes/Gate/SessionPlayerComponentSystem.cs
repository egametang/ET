namespace ET.Server
{
    public static partial class SessionPlayerComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this SessionPlayerComponent self)
        {
            if (ActorLocationSenderComponent.Instance.IsDisposed)
            {
                return;
            }
            // 发送断线消息
            ActorLocationSenderComponent.Instance.Get(LocationType.Unit).Send(self.Player.Id, new G2M_SessionDisconnect());
        }
    }
}