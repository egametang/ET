using System;

namespace ET
{
    [ProcessActorHandler]
    public abstract class ProcessActorHandler<T>: IProcessActorHandler where T : MessageObject, IProcessActorMessage
    {
        protected abstract void Run(T messageObject);

        public void Handle(MessageObject messageObject)
        {
            this.Run((T)messageObject);
        }

        public Type GetMessageType()
        {
            return typeof (T);
        }
    }
    
    [ProcessActorHandler]
    public abstract class ProcessActorHandler<Request, Response>: IProcessActorHandler where Request : MessageObject, IProcessActorRequest where Response: MessageObject, IProcessActorResponse, new()
    {
        protected abstract void Run(Request request, Response response);

        public void Handle(MessageObject messageObject)
        {
            Request request = (Request)messageObject;
            int processId = request.ProcessId;
            Response response = new();
            this.Run(request, response);
            IvVProcessActor.Instance.Send(processId, response);
        }

        public Type GetMessageType()
        {
            return typeof (Request);
        }
        
        public Type GetResponseType()
        {
            return typeof (Response);
        }
    }
}