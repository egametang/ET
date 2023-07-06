using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [FriendOf(typeof(ActorSenderComponent))]
    public static partial class ActorSenderComponentSystem
    {
        public static void Send(this ActorSenderComponent self, ActorId actorId, IActorMessage message)
        {
            Fiber fiber = self.Fiber();
            // 如果发向同一个进程，则扔到消息队列中
            if (actorId.Process == fiber.Process)
            {
                ActorMessageQueue.Instance.Send(fiber.Address, actorId, (MessageObject)message);
                return;
            }
            
            // 发给NetInner纤程
            A2NetInner_Message a2NetInnerMessage = A2NetInner_Message.Create();
            a2NetInnerMessage.FromAddress = fiber.Address;
            a2NetInnerMessage.ActorId = actorId;
            a2NetInnerMessage.MessageObject = message;

            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.NetInners[fiber.Process];
            ActorMessageQueue.Instance.Send(startSceneConfig.ActorId, a2NetInnerMessage);
        }

        public static int GetRpcId(this ActorSenderComponent self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IActorResponse> Call(
                this ActorSenderComponent self,
                ActorId actorId,
                IActorRequest request,
                bool needException = true
        )
        {
            request.RpcId = self.GetRpcId();
            
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {request}");
            }

            return await self.Call(actorId, request.RpcId, request, needException);
        }
        
        public static async ETTask<IActorResponse> Call(
                this ActorSenderComponent self,
                ActorId actorId,
                int rpcId,
                IActorRequest iActorRequest,
                bool needException = true
        )
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {iActorRequest}");
            }
            Fiber fiber = self.Fiber();
            if (fiber.Process == actorId.Process)
            {
                return await fiber.Root.GetComponent<ActorInnerComponent>().Call(actorId, rpcId, iActorRequest, needException);
            }
            
            // 发给NetInner纤程
            A2NetInner_Request a2NetInner_Request = A2NetInner_Request.Create();
            a2NetInner_Request.ActorId = actorId;
            a2NetInner_Request.MessageObject = iActorRequest;
            a2NetInner_Request.NeedException = needException;
            StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.NetInners[fiber.Process];
            A2NetInner_Response response = await fiber.Root.GetComponent<ActorSenderComponent>().Call(
                startSceneConfig.ActorId, a2NetInner_Request, needException: a2NetInner_Request.NeedException) as A2NetInner_Response;
            return response.MessageObject;
        }
    }
}