using System;

namespace ET.Server
{
    public abstract class MessageLocationHandler<E, Message>: HandlerObject, IMHandler where E : Entity where Message : class, ILocationMessage
    {
        protected abstract ETTask Run(E entity, Message message);

        public async ETTask Handle(Entity entity, Address fromAddress, MessageObject actorMessage)
        {
            Fiber fiber = entity.Fiber();
            if (actorMessage is not Message message)
            {
                Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Message).Name}");
                return;
            }

            if (entity is not E e)
            {
                Log.Error($"Actor类型转换错误: {entity.GetType().FullName} to {typeof (E).FullName} --{typeof (Message).FullName}");
                return;
            }
            
            MessageResponse response = ObjectPool.Instance.Fetch<MessageResponse>();
            response.RpcId = message.RpcId;
            fiber.Root.GetComponent<ProcessInnerSender>().Reply(fromAddress, response);

            await this.Run(e, message);
        }

        public Type GetRequestType()
        {
            return typeof (Message);
        }

        public Type GetResponseType()
        {
            return typeof (MessageResponse);
        }
    }
    
    
    
    public abstract class MessageLocationHandler<E, Request, Response>: HandlerObject, IMHandler where E : Entity where Request : MessageObject, ILocationRequest where Response : MessageObject, ILocationResponse
    {
        protected abstract ETTask Run(E unit, Request request, Response response);

        public async ETTask Handle(Entity entity, Address fromAddress, MessageObject actorMessage)
        {
            try
            {
                Fiber fiber = entity.Fiber();
                if (actorMessage is not Request request)
                {
                    Log.Error($"消息类型转换错误: {actorMessage.GetType().FullName} to {typeof (Request).Name}");
                    return;
                }

                if (entity is not E ee)
                {
                    Log.Error($"Actor类型转换错误: {entity.GetType().FullName} to {typeof (E).FullName} --{typeof (Request).FullName}");
                    return;
                }

                int rpcId = request.RpcId;
                Response response = ObjectPool.Instance.Fetch<Response>();
                try
                {
                    await this.Run(ee, request, response);
                }
                catch (RpcException exception)
                {
                    response.Error = exception.Error;
                    response.Message = exception.ToString();
                }
                catch (Exception exception)
                {
                    response.Error = ErrorCore.ERR_RpcFail;
                    response.Message = exception.ToString();
                }
                response.RpcId = rpcId;
                fiber.Root.GetComponent<ProcessInnerSender>().Reply(fromAddress, response);
            }
            catch (Exception e)
            {
                throw new Exception($"解释消息失败: {actorMessage.GetType().FullName}", e);
            }
        }

        public Type GetRequestType()
        {
            return typeof (Request);
        }

        public Type GetResponseType()
        {
            return typeof (Response);
        }
    }
}