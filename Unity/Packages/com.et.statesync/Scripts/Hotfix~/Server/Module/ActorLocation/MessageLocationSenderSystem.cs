using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(MessageLocationSender))]
    public static partial class MessageLocationSenderSystem
    {
        [EntitySystem]
        private static void Awake(this MessageLocationSender self)
        {
            self.LastSendOrRecvTime = TimeInfo.Instance.ServerNow();
            self.ActorId = default;
        }
        
        [EntitySystem]
        private static void Destroy(this MessageLocationSender self)
        {
            Log.Debug($"actor location remove: {self.Id}");
            self.LastSendOrRecvTime = 0;
            self.ActorId = default;
        }
    }
}