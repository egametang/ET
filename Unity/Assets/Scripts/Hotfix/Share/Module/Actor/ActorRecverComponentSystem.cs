namespace ET
{
    [FriendOf(typeof(ActorRecverComponent))]
    public static partial class ActorRecverComponentSystem
    {
        [EntitySystem]
        private static void Destroy(this ActorRecverComponent self)
        {
            ActorMessageQueue.Instance.RemoveQueue((int)self.Fiber().Id);
        }

        [EntitySystem]
        private static void Awake(this ActorRecverComponent self)
        {
            ActorMessageQueue.Instance.AddQueue((int)self.Fiber().Id);
        }

        [EntitySystem]
        private static void Update(this ActorRecverComponent self)
        {
            self.list.Clear();
            Fiber fiber = self.Fiber();
            ActorMessageQueue.Instance.Fetch(fiber.Id, 1000, self.list);

            ActorSenderComponent actorSenderComponent = fiber.Root.GetComponent<ActorSenderComponent>();
            foreach (ActorMessageInfo actorMessageInfo in self.list)
            {
                if (actorMessageInfo.MessageObject is IActorResponse response)
                {
                    actorSenderComponent.HandleIActorResponse(response);
                    continue;
                }

                ActorId actorId = actorMessageInfo.ActorId;
                MessageObject message = actorMessageInfo.MessageObject;

                MailBoxComponent mailBoxComponent = self.Fiber().Mailboxes.Get(actorId.InstanceId);
                if (mailBoxComponent == null)
                {
                    Log.Warning($"actor not found mailbox: {actorId} {message}");
                    if (message is IActorRequest request)
                    {
                        IActorResponse resp = ActorHelper.CreateResponse(request, ErrorCore.ERR_NotFoundActor);
                        actorSenderComponent.Reply(actorId.Address, resp);
                    }
                    return;
                }
                mailBoxComponent.Add(actorId.Address, message);
            }
        }
    }
}