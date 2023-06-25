using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(Fiber))]
    public class FiberActor: SingletonEntity<FiberActor>, IAwake
    {
        private readonly Dictionary<int, ETTask<IResponse>> requestCallbacks = new();

        private readonly List<ActorMessageInfo> list = new();

        private int rpcId;
        
        public override void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }
            
            base.Dispose();
            
            ActorMessageQueue.Instance.RemoveQueue((int)this.Root().Id);
        }

        public void Awake()
        {
            ActorMessageQueue.Instance.AddQueue((int)this.Root().Id);
        }

        public void Update()
        {
            this.list.Clear();
            Fiber fiber = this.Root();
            ActorMessageQueue.Instance.Fetch((int)fiber.Id, 1000, this.list);
            
            ActorMessageDispatcherComponent actorMessageDispatcherComponent = ActorMessageDispatcherComponent.Instance;
            foreach (ActorMessageInfo actorMessageInfo in this.list)
            {
                if (actorMessageInfo.MessageObject is IResponse response)
                {
                    HandleResponse(response);
                    continue;
                }

                Entity entity = fiber.ActorEntities.Get(actorMessageInfo.ActorId);
                actorMessageDispatcherComponent.Handle(entity, actorMessageInfo.ActorId, actorMessageInfo.MessageObject).Coroutine();    
            }
        }

        private void HandleResponse(IResponse response)
        {
            
        }
        
        public void Send(ActorId actorId, MessageObject messageObject)
        {
            Fiber fiber = this.Root();
            ActorMessageQueue.Instance.Send(new Address(fiber.Process, (int)fiber.Id), actorId, messageObject);
        }
    }
}