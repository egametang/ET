using System.Collections.Generic;

namespace ET
{
    [ComponentOf(typeof(VProcess))]
    public class VProcessActor: SingletonEntity<VProcessActor>, IAwake
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
            
            ActorQueue.Instance.RemoveQueue((int)this.Root().Id);
        }

        public void Awake()
        {
            ActorQueue.Instance.AddQueue((int)this.Root().Id);
        }

        public void Update()
        {
            this.list.Clear();
            VProcess vProcess = this.Root();
            ActorQueue.Instance.Fetch((int)vProcess.Id, 1000, this.list);
            
            ActorMessageDispatcherComponent actorMessageDispatcherComponent = ActorMessageDispatcherComponent.Instance;
            foreach (ActorMessageInfo actorMessageInfo in this.list)
            {
                if (actorMessageInfo.MessageObject is IResponse response)
                {
                    HandleResponse(response);
                    continue;
                }

                Entity entity = vProcess.ActorEntities.Get(actorMessageInfo.ActorId);
                actorMessageDispatcherComponent.Handle(entity, actorMessageInfo.ActorId, actorMessageInfo.MessageObject).Coroutine();    
            }
        }

        private void HandleResponse(IResponse response)
        {
            
        }
        
        public void Send(ActorId actorId, MessageObject messageObject)
        {
            VProcess vProcess = this.Root();
            ActorQueue.Instance.Send(new Address(vProcess.Process, (int)vProcess.Id), actorId, messageObject);
        }
    }
}