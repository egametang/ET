using System.Collections.Generic;

namespace ET
{
    public class VProcessActor: VProcessSingleton<VProcessActor>, IVProcessSingletonAwake
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
            
            WorldActor.Instance.RemoveActor(this.VProcess.Id);
        }

        public void Awake()
        {
            WorldActor.Instance.AddActor(this.VProcess.Id);
        }

        public void Update()
        {
            this.list.Clear();
            WorldActor.Instance.Fetch(this.VProcess.Id, 1000, this.list);
            foreach (ActorMessageInfo actorMessageInfo in this.list)
            {
                this.HandleMessage(actorMessageInfo.ActorId, actorMessageInfo.MessageObject);    
            }
        }
        
        private void HandleMessage(ActorId actorId, MessageObject messageObject)
        {
            switch (messageObject)
            {
                case IResponse iResponse:
                {
                    if (this.requestCallbacks.TryGetValue(iResponse.RpcId, out ETTask<IResponse> task))
                    {
                        task.SetResult(iResponse);
                    }
                    break;
                }
                case IRequest iRequest:
                {
                    WorldActor.Instance.Handle(actorId, messageObject);
                    break;
                }
                default: // IMessage:
                {
                    WorldActor.Instance.Handle(actorId, messageObject);
                    break;
                }
            }
        }
        
        public void Send(ActorId actorId, MessageObject messageObject)
        {
            WorldActor.Instance.Send(actorId, messageObject);
        }
        
        public async ETTask<IResponse> Call(ActorId actorId, IRequest request)
        {
            ETTask<IResponse> task = ETTask<IResponse>.Create(true);
            request.RpcId = ++this.rpcId;
            this.requestCallbacks.Add(request.RpcId, task);
            this.Send(actorId, request as MessageObject);
            return await task;
        }
    }
}