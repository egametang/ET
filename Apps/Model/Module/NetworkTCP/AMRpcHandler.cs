using System;

namespace ET
{
    [MessageHandler]
    public abstract class AMRpcHandler<Request, Response>: IMHandler where Request : class, IRequest where Response : class, IResponse
    {
        protected abstract ETTask Run(Session session, Request request, Response response, Action reply);

        public void Handle(Session session, object message)
        {
            HandleAsync(session, message).Coroutine();
        }

        private async ETTask HandleAsync(Session session, object message)
        {
            try
            {
                Request request = message as Request;
                if (request == null)
                {
                    throw new Exception($"消息类型转换错误: {message.GetType().Name} to {typeof (Request).Name}");
                }

                int rpcId = request.RpcId;

                long instanceId = session.InstanceId;

                Response response = Activator.CreateInstance<Response>();

                void Reply()
                {
                    // 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
                    if (session.InstanceId != instanceId)
                    {
                        return;
                    }

                    response.RpcId = rpcId;
                    session.Reply(response);
                }

                try
                {
                    await this.Run(session, request, response, Reply);
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                    Reply();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"解释消息失败: {message.GetType().FullName}", e);
            }
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