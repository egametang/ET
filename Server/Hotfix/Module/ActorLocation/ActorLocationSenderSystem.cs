using System;

namespace ET
{
    [ObjectSystem]
    public class ActorLocationSenderAwakeSystem: AwakeSystem<ActorLocationSender>
    {
        public override void Awake(ActorLocationSender self)
        {
            self.LastSendOrRecvTime = TimeHelper.ServerNow();
            self.ActorId = 0;
            self.Error = 0;
        }
    }

    [ObjectSystem]
    public class ActorLocationSenderDestroySystem: DestroySystem<ActorLocationSender>
    {
        public override void Destroy(ActorLocationSender self)
        {
            Log.Debug($"actor location remove: {self.Id}");
            self.LastSendOrRecvTime = 0;
            self.ActorId = 0;
            self.Error = 0;
        }
    }
}