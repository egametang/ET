using ET.Server;

namespace ET
{
    [FriendOf(typeof(ActorMessageRecvComponent))]
    public static partial class ActorMessageRecvComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this ActorMessageRecvComponent self)
        {
            ActorMessageQueue.Instance.RemoveQueue((int)self.Fiber().Id);
        }

        [EntitySystem]
        private static void Awake(this ActorMessageRecvComponent self)
        {
            ActorMessageQueue.Instance.AddQueue((int)self.Fiber().Id);
        }

        [EntitySystem]
        private static void Update(this ActorMessageRecvComponent self)
        {
            self.list.Clear();
            Fiber fiber = self.Fiber();
            ActorMessageQueue.Instance.Fetch((int)fiber.Id, 1000, self.list);

            ActorMessageSenderComponent actorMessageSenderComponent = self.Fiber().GetComponent<ActorMessageSenderComponent>();
            foreach (ActorMessageInfo actorMessageInfo in self.list)
            {
                if (actorMessageInfo.MessageObject is IActorResponse response)
                {
                    actorMessageSenderComponent.HandleIActorResponse(response);
                    continue;
                }

                ActorId actorId = actorMessageInfo.ActorId;
                MessageObject message = actorMessageInfo.MessageObject;
                actorId.Address = fiber.Address;
                MailBoxComponent mailBoxComponent = self.Fiber().Mailboxes.Get(actorId.InstanceId);
                if (mailBoxComponent == null)
                {
                    Log.Warning($"actor not found mailbox: {actorId} {message}");
                    IActorResponse resp = ActorHelper.CreateResponse((IActorRequest)message, ErrorCore.ERR_NotFoundActor);
                    actorMessageSenderComponent.Reply(actorId, resp);
                    return;
                }
                mailBoxComponent.Add(message);
            }
        }
    }
}