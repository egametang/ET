using System;

namespace ET.Server
{
    [FriendOf(typeof(ActorLocationSender))]
    public static class ActorLocationSenderSystem
    {
        public class ActorLocationSenderAwakeSystem: AwakeSystem<ActorLocationSender>
        {
            protected override void Awake(ActorLocationSender self)
            {
                self.Awake();
            }
        }

        public class ActorLocationSenderDestroySystem: DestroySystem<ActorLocationSender>
        {
            protected override void Destroy(ActorLocationSender self)
            {
                self.Destroy();
            }
        }
        
        private static void Awake(this ActorLocationSender self)
        {
            self.LastSendOrRecvTime = TimeHelper.ServerNow();
            self.ActorId = 0;
            self.Error = 0;
        }
        
        private static void Destroy(this ActorLocationSender self)
        {
            Log.Debug($"actor location remove: {self.Id}");
            self.LastSendOrRecvTime = 0;
            self.ActorId = 0;
            self.Error = 0;
        }
    }
}