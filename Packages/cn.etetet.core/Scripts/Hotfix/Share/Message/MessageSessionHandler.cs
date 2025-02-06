using System;

namespace ET
{
    public abstract class MessageSessionHandler<Message>: HandlerObject, IMessageSessionHandler where Message : MessageObject
    {
        protected abstract ETTask Run(Session session, Message message);

        public void Handle(Session session, object msg)
        {
            HandleAsync(session, msg).NoContext();
        }

        private async ETTask HandleAsync(Session session, object message)
        {
            if (message == null)
            {
                Log.Error($"消息类型转换错误: {message.GetType().FullName} to {typeof (Message).Name}");
                return;
            }

            if (session.IsDisposed)
            {
                Log.Error($"session disconnect {message}");
                return;
            }

            await this.Run(session, (Message)message);
        }

        public Type GetMessageType()
        {
            return typeof (Message);
        }

        public Type GetResponseType()
        {
            return null;
        }
    }
    
    public abstract class MessageSessionHandler<Request, Response>: HandlerObject, IMessageSessionHandler where Request : MessageObject, IRequest where Response : MessageObject, IResponse
    {
        protected abstract ETTask Run(Session session, Request request, Response response);

        public void Handle(Session session, object message)
        {
            HandleAsync(session, message).NoContext();
        }

        private async ETTask HandleAsync(Session session, object message)
        {
            try
            {
                Request request = message as Request;
                if (request == null)
                {
                    throw new Exception($"消息类型转换错误: {message.GetType().FullName} to {typeof (Request).FullName}");
                }

                int rpcId = request.RpcId;
                long instanceId = session.InstanceId;

                // 这里用using很安全，因为后面是session发送出去了
                using Response response = ObjectPool.Fetch<Response>();
                try
                {
                    await this.Run(session, request, response);
                }
                catch (RpcException exception)
                {
                    // 这里不能返回堆栈给客户端
                    Log.Error(exception.ToString());
                    response.Error = exception.Error;
                }
                catch (Exception exception)
                {
                    // 这里不能返回堆栈给客户端
                    Log.Error(exception.ToString());
                    response.Error = ErrorCode.ERR_RpcFail;
                }
                
                // 等回调回来,session可以已经断开了,所以需要判断session InstanceId是否一样
                if (session.InstanceId != instanceId)
                {
                    return;
                }
                
                response.RpcId = rpcId; // 在这里设置rpcId是为了防止在Run中不小心修改rpcId字段
                session.Send(response);
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