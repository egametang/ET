using System;

namespace ET.Server
{
    [FriendOf(typeof(ActorLocationSender))]
    public static partial class ActorLocationSenderSystem
    {
        [EntitySystem]
        private static void Awake(this ActorLocationSender self)
        {
            self.LastSendOrRecvTime = TimeHelper.ServerNow();
            self.ActorId = default;
            self.Error = 0;
        }
        
        [EntitySystem]
        private static void Destroy(this ActorLocationSender self)
        {
            Log.Debug($"actor location remove: {self.Id}");
            self.LastSendOrRecvTime = 0;
            self.ActorId = default;
            self.Error = 0;
        }
    }
}