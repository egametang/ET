﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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
                fiber.ProcessInnerSender.Send(actorId, message);
                return;
            }
            
            // 发给NetInner纤程
            A2NetInner_Message a2NetInnerMessage = A2NetInner_Message.Create();
            a2NetInnerMessage.FromAddress = fiber.Address;
            a2NetInnerMessage.ActorId = actorId;
            a2NetInnerMessage.MessageObject = message;

            MessageQueue.Instance.Send(new ActorId(fiber.Process, ConstFiberId.NetInner), a2NetInnerMessage);
        }

        public static int GetRpcId(this MessageSender self)
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
            request.RpcId = self.GetRpcId();
            
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {request}");
            }

            return await self.Call(actorId, request.RpcId, request, needException);
        }
        
        public static async ETTask<IResponse> Call(
                this MessageSender self,
                ActorId actorId,
                int rpcId,
                IRequest request,
                bool needException = true
        )
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {request}");
            }
            Fiber fiber = self.Fiber();
            
            if (fiber.Process == actorId.Process)
            {
                return await fiber.ProcessInnerSender.Call(actorId, rpcId, request, needException: needException);
            }

            // 发给NetInner纤程
            A2NetInner_Request a2NetInner_Request = A2NetInner_Request.Create();
            a2NetInner_Request.ActorId = actorId;
            a2NetInner_Request.MessageObject = request;
            
            A2NetInner_Response a2NetInnerResponse = await fiber.ProcessInnerSender.Call(
                new ActorId(fiber.Process, ConstFiberId.NetInner), a2NetInner_Request) as A2NetInner_Response;
            IResponse response = a2NetInnerResponse.MessageObject;
            
            if (response.Error == ErrorCore.ERR_MessageTimeout)
            {
                throw new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {actorId} {request}, response: {response}");
            }
            if (needException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                throw new RpcException(response.Error, $"Rpc error: actorId: {actorId} {request}, response: {response}");
            }
            return response;
        }
    }
}