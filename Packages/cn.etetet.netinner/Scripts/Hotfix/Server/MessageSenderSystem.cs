using System;
using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(MessageSender))]
    public static partial class MessageSenderSystem
    {
        public static void Send(this MessageSender self, ActorId actorId, IMessage message)
        {
            Fiber fiber = self.Fiber();
            // 如果发向同一个进程，则扔到消息队列中
            if (actorId.Process == fiber.Process)
            {
                fiber.Root.GetComponent<ProcessInnerSender>().Send(actorId, message);
                return;
            }
            
            // 发给NetInner纤程
            A2NetInner_Message a2NetInnerMessage = A2NetInner_Message.Create();
            a2NetInnerMessage.FromAddress = fiber.Address;
            a2NetInnerMessage.ActorId = actorId;
            a2NetInnerMessage.MessageObject = message;

            MessageQueue.Instance.Send(new ActorId(fiber.Process, SceneType.NetInner), a2NetInnerMessage);
        }

        private static int GetRpcId(this MessageSender self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IResponse> Call(
                this MessageSender self,
                ActorId actorId,
                IRequest request,
                bool needException = true
        )
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {request}");
            }
            Fiber fiber = self.Fiber();

            IResponse response;
            if (fiber.Process == actorId.Process)
            {
                response = await fiber.Root.GetComponent<ProcessInnerSender>().Call(actorId, request, needException: needException);
            }
            else
            {
                // 发给NetInner纤程
                A2NetInner_Request a2NetInner_Request = A2NetInner_Request.Create();
                a2NetInner_Request.ActorId = actorId;
                a2NetInner_Request.MessageObject = request;
            
                using A2NetInner_Response a2NetInnerResponse = await fiber.Root.GetComponent<ProcessInnerSender>().Call(
                    new ActorId(fiber.Process, SceneType.NetInner), a2NetInner_Request) as A2NetInner_Response;
                response = a2NetInnerResponse.MessageObject;
            }
            
            if (response.Error == ErrorCode.ERR_MessageTimeout)
            {
                throw new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {actorId} {request}, response: {response}");
            }
            if (needException && ErrorCode.IsRpcNeedThrowException(response.Error))
            {
                throw new RpcException(response.Error, $"Rpc error: actorId: {actorId} {request}, response: {response}");
            }
            return response;
        }
    }
}