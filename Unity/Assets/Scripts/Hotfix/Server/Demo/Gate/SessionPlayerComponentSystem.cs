namespace ET.Server
{
    public static partial class SessionPlayerComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this SessionPlayerComponent self)
        {
            if (self.Fiber().IsDisposed)
            {
                return;
            }
            // 发送断线消息
            self.Fiber().GetComponent<ActorLocationSenderComponent>().Get(LocationType.Unit).Send(self.Player.Id, new G2M_SessionDisconnect());
        }
    }
}