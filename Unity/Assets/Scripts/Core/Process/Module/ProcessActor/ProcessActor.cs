using System.Collections.Generic;

namespace ET
{
    public class ProcessActor: ProcessSingleton<ProcessActor>, IProcessSingletonUpdate, IProcessSingletonAwake
    {
        private readonly Dictionary<int, ETTask<IResponse>> requestCallbacks = new();

        private readonly List<MessageObject> list = new();

        private int rpcId;
        
        public void Awake()
        {
            GameActor.Instance.AddActor(this.Process.Id);
        }

        public override void Dispose()
        {
            GameActor.Instance.RemoveActor(this.Process.Id);
        }

        public void Update()
        {
            this.list.Clear();
            GameActor.Instance.Fetch(this.Process.Id, 1000, this.list);
            foreach (MessageObject messageObject in this.list)
            {
                this.HandleMessage(messageObject);    
            }
        }
        
        private void HandleMessage(MessageObject messageObject)
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
                    GameActor.Instance.Handle(messageObject);
                    break;
                }
                default: // IMessage:
                {
                    GameActor.Instance.Handle(messageObject);
                    break;
                }
            }
        }
        
        public void Send(int processId, MessageObject messageObject)
        {
            GameActor.Instance.Send(processId, messageObject);
        }
        
        public async ETTask<IResponse> Call(int processId, IRequest request)
        {
            ETTask<IResponse> task = ETTask<IResponse>.Create(true);
            request.RpcId = ++this.rpcId;
            this.requestCallbacks.Add(request.RpcId, task);
            this.Send(processId, request as MessageObject);
            return await task;
        }
    }
}