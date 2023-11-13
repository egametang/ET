using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    [EntitySystemOf(typeof(ProcessInnerSender))]
    [FriendOf(typeof(ProcessInnerSender))]
    public static partial class ProcessInnerSenderSystem
    {
        [EntitySystem]
        private static void Destroy(this ProcessInnerSender self)
        {
            Fiber fiber = self.Fiber();
            MessageQueue.Instance.RemoveQueue(fiber.Id);
        }

        [EntitySystem]
        private static void Awake(this ProcessInnerSender self)
        {
            Fiber fiber = self.Fiber();
            MessageQueue.Instance.AddQueue(fiber.Id);
        }

        [EntitySystem]
        private static void Update(this ProcessInnerSender self)
        {
            self.list.Clear();
            Fiber fiber = self.Fiber();
            MessageQueue.Instance.Fetch(fiber.Id, 1000, self.list);

            foreach (MessageInfo actorMessageInfo in self.list)
            {
                self.HandleMessage(fiber, actorMessageInfo);
            }
        }

        private static void HandleMessage(this ProcessInnerSender self, Fiber fiber, in MessageInfo messageInfo)
        {
            if (messageInfo.MessageObject is IResponse response)
            {
                self.HandleIActorResponse(response);
                return;
            }

            ActorId actorId = messageInfo.ActorId;
            MessageObject message = messageInfo.MessageObject;

            MailBoxComponent mailBoxComponent = self.Fiber().Mailboxes.Get(actorId.InstanceId);
            if (mailBoxComponent == null)
            {
                Log.Warning($"actor not found mailbox, from: {actorId} current: {fiber.Address} {message}");
                if (message is IRequest request)
                {
                    IResponse resp = MessageHelper.CreateResponse(request, ErrorCore.ERR_NotFoundActor);
                    self.Reply(actorId.Address, resp);
                }
                message.Dispose();
                return;
            }
            mailBoxComponent.Add(actorId.Address, message);
        }

        private static void HandleIActorResponse(this ProcessInnerSender self, IResponse response)
        {
            if (!self.requestCallback.Remove(response.RpcId, out MessageSenderStruct actorMessageSender))
            {
                return;
            }
            Run(actorMessageSender, response);
        }
        
        private static void Run(MessageSenderStruct self, IResponse response)
        {
            if (response.Error == ErrorCore.ERR_MessageTimeout)
            {
                self.Tcs.SetException(new RpcException(response.Error, $"Rpc error: request, 注意Actor消息超时，请注意查看是否死锁或者没有reply: actorId: {self.ActorId} {self.Request}, response: {response}"));
                return;
            }

            if (self.NeedException && ErrorCore.IsRpcNeedThrowException(response.Error))
            {
                self.Tcs.SetException(new RpcException(response.Error, $"Rpc error: actorId: {self.ActorId} request: {self.Request}, response: {response}"));
                return;
            }

            self.Tcs.SetResult(response);
            ((MessageObject)response).Dispose();
        }
        
        public static void Reply(this ProcessInnerSender self, Address fromAddress, IResponse message)
        {
            self.SendInner(new ActorId(fromAddress, 0), (MessageObject)message);
        }

        public static void Send(this ProcessInnerSender self, ActorId actorId, IMessage message)
        {
            self.SendInner(actorId, (MessageObject)message);
        }

        private static void SendInner(this ProcessInnerSender self, ActorId actorId, MessageObject message)
        {
            Fiber fiber = self.Fiber();
            
            // 如果发向同一个进程，则扔到消息队列中
            if (actorId.Process != fiber.Process)
            {
                throw new Exception($"actor inner process diff: {actorId.Process} {fiber.Process}");
            }

            if (actorId.Fiber == fiber.Id)
            {
                self.HandleMessage(fiber, new MessageInfo() {ActorId = actorId, MessageObject = message});
                return;
            }
            
            MessageQueue.Instance.Send(fiber.Address, actorId, message);
        }

        public static int GetRpcId(this ProcessInnerSender self)
        {
            return ++self.RpcId;
        }

        public static async ETTask<IResponse> Call(
                this ProcessInnerSender self,
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
                this ProcessInnerSender self,
                ActorId actorId,
                int rpcId,
                IRequest iRequest,
                bool needException = true
        )
        {
            if (actorId == default)
            {
                throw new Exception($"actor id is 0: {iRequest}");
            }
            Fiber fiber = self.Fiber();
            if (fiber.Process != actorId.Process)
            {
                throw new Exception($"actor inner process diff: {actorId.Process} {fiber.Process}");
            }
            
            var tcs = ETTask<IResponse>.Create(true);

            self.requestCallback.Add(rpcId, new MessageSenderStruct(actorId, iRequest, tcs, needException));
            
            self.SendInner(actorId, (MessageObject)iRequest);

            
            async ETTask Timeout()
            {
                await fiber.Root.GetComponent<TimerComponent>().WaitAsync(ProcessInnerSender.TIMEOUT_TIME);

                if (!self.requestCallback.Remove(rpcId, out MessageSenderStruct action))
                {
                    return;
                }
                
                if (needException)
                {
                    action.Tcs.SetException(new Exception($"actor sender timeout: {iRequest}"));
                }
                else
                {
                    IResponse response = MessageHelper.CreateResponse(iRequest, ErrorCore.ERR_Timeout);
                    action.Tcs.SetResult(response);
                }
            }
            
            Timeout().Coroutine();
            
            long beginTime = TimeInfo.Instance.ServerFrameTime();

            IResponse response = await tcs;
            
            long endTime = TimeInfo.Instance.ServerFrameTime();

            long costTime = endTime - beginTime;
            if (costTime > 200)
            {
                Log.Warning($"actor rpc time > 200: {costTime} {iRequest}");
            }
            
            return response;
        }
    }
}