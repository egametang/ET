﻿using System;
using System.IO;
using MongoDB.Bson;

namespace ET.Server
{
    [EntitySystemOf(typeof(MessageLocationSenderOneType))]
    [FriendOf(typeof(MessageLocationSenderOneType))]
    [FriendOf(typeof(MessageLocationSender))]
    public static partial class MessageLocationSenderComponentSystem
    {
        [Invoke(TimerInvokeType.MessageLocationSenderChecker)]
        public class MessageLocationSenderChecker: ATimer<MessageLocationSenderOneType>
        {
            protected override void Run(MessageLocationSenderOneType self)
            {
                try
                {
                    self.Check();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
    
        [EntitySystem]
        private static void Awake(this MessageLocationSenderOneType self, int locationType)
        {
            self.LocationType = locationType;
            // 每10s扫描一次过期的actorproxy进行回收,过期时间是2分钟
            // 可能由于bug或者进程挂掉，导致ActorLocationSender发送的消息没有确认，结果无法自动删除，每一分钟清理一次这种ActorLocationSender
            self.CheckTimer = self.Fiber().TimerComponent.NewRepeatedTimer(10 * 1000, TimerInvokeType.MessageLocationSenderChecker, self);
        }
        
        [EntitySystem]
        private static void Destroy(this MessageLocationSenderOneType self)
        {
            self.Fiber().TimerComponent?.Remove(ref self.CheckTimer);
        }

        private static void Check(this MessageLocationSenderOneType self)
        {
            using (ListComponent<long> list = ListComponent<long>.Create())
            {
                long timeNow = TimeInfo.Instance.ServerNow();
                foreach ((long key, Entity value) in self.Children)
                {
                    MessageLocationSender messageLocationMessageSender = (MessageLocationSender) value;

                    if (timeNow > messageLocationMessageSender.LastSendOrRecvTime + MessageLocationSenderOneType.TIMEOUT_TIME)
                    {
                        list.Add(key);
                    }
                }

                foreach (long id in list)
                {
                    self.Remove(id);
                }
            }
        }

        private static MessageLocationSender GetOrCreate(this MessageLocationSenderOneType self, long id)
        {
            if (id == 0)
            {
                throw new Exception($"actor id is 0");
            }

            if (self.Children.TryGetValue(id, out Entity actorLocationSender))
            {
                return (MessageLocationSender) actorLocationSender;
            }

            actorLocationSender = self.AddChildWithId<MessageLocationSender>(id);
            return (MessageLocationSender) actorLocationSender;
        }

        // 有需要主动删除actorMessageSender的需求，比如断线重连，玩家登录了不同的Gate，这时候需要通知map删掉之前的actorMessageSender
        // 然后重新创建新的，重新请求新的ActorId
        public static void Remove(this MessageLocationSenderOneType self, long id)
        {
            if (!self.Children.TryGetValue(id, out Entity actorMessageSender))
            {
                return;
            }

            actorMessageSender.Dispose();
        }
        
        // 发给不会改变位置的actorlocation用这个，这种actor消息不会阻塞发送队列，性能更高
        // 发送过去找不到actor不会重试,用此方法，你得保证actor提前注册好了location
        public static void Send(this MessageLocationSenderOneType self, long entityId, IMessage message)
        {
            self.SendInner(entityId, message).Coroutine();
        }
        
        private static async ETTask SendInner(this MessageLocationSenderOneType self, long entityId, IMessage message)
        {
            MessageLocationSender messageLocationSender = self.GetOrCreate(entityId);

            Scene root = self.Root();
            
            if (messageLocationSender.ActorId != default)
            {
                messageLocationSender.LastSendOrRecvTime = TimeInfo.Instance.ServerNow();
                root.GetComponent<MessageSender>().Send(messageLocationSender.ActorId, message);
                return;
            }
            
            long instanceId = messageLocationSender.InstanceId;
            
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.MessageLocationSender;
            using (await root.Fiber.CoroutineLockComponent.Wait(coroutineLockType, entityId))
            {
                if (messageLocationSender.InstanceId != instanceId)
                {
                    throw new RpcException(ErrorCore.ERR_MessageTimeout, $"{message}");
                }
                
                if (messageLocationSender.ActorId == default)
                {
                    messageLocationSender.ActorId = await root.GetComponent<LocationProxyComponent>().Get(self.LocationType, messageLocationSender.Id);
                    if (messageLocationSender.InstanceId != instanceId)
                    {
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{message}");
                    }
                }
                
                messageLocationSender.LastSendOrRecvTime = TimeInfo.Instance.ServerNow();
                root.GetComponent<MessageSender>().Send(messageLocationSender.ActorId, message);
            }
        }

        // 发给不会改变位置的actorlocation用这个，这种actor消息不会阻塞发送队列，性能更高，发送过去找不到actor不会重试
        // 发送过去找不到actor不会重试,用此方法，你得保证actor提前注册好了location
        public static async ETTask<IResponse> Call(this MessageLocationSenderOneType self, long entityId, IRequest request)
        {
            MessageLocationSender messageLocationSender = self.GetOrCreate(entityId);

            Scene root = self.Root();
            
            if (messageLocationSender.ActorId != default)
            {
                messageLocationSender.LastSendOrRecvTime = TimeInfo.Instance.ServerNow();
                return await root.GetComponent<MessageSender>().Call(messageLocationSender.ActorId, request);
            }
            
            long instanceId = messageLocationSender.InstanceId;
            
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.MessageLocationSender;
            using (await root.Fiber.CoroutineLockComponent.Wait(coroutineLockType, entityId))
            {
                if (messageLocationSender.InstanceId != instanceId)
                {
                    throw new RpcException(ErrorCore.ERR_MessageTimeout, $"{request}");
                }

                if (messageLocationSender.ActorId == default)
                {
                    messageLocationSender.ActorId = await root.GetComponent<LocationProxyComponent>().Get(self.LocationType, messageLocationSender.Id);
                    if (messageLocationSender.InstanceId != instanceId)
                    {
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{request}");
                    }
                }
            }

            messageLocationSender.LastSendOrRecvTime = TimeInfo.Instance.ServerNow();
            return await root.GetComponent<MessageSender>().Call(messageLocationSender.ActorId, request);
        }

        public static void Send(this MessageLocationSenderOneType self, long entityId, ILocationMessage message)
        {
            self.Call(entityId, message).Coroutine();
        }

        public static async ETTask<IResponse> Call(this MessageLocationSenderOneType self, long entityId, ILocationRequest iRequest)
        {
            MessageLocationSender messageLocationSender = self.GetOrCreate(entityId);

            Scene root = self.Root();
            
            int rpcId = root.GetComponent<MessageSender>().GetRpcId();
            iRequest.RpcId = rpcId;
            
            long actorLocationSenderInstanceId = messageLocationSender.InstanceId;
            int coroutineLockType = (self.LocationType << 16) | CoroutineLockType.MessageLocationSender;
            using (await root.Fiber.CoroutineLockComponent.Wait(coroutineLockType, entityId))
            {
                if (messageLocationSender.InstanceId != actorLocationSenderInstanceId)
                {
                    throw new RpcException(ErrorCore.ERR_MessageTimeout, $"{iRequest}");
                }

                // 队列中没处理的消息返回跟上个消息一样的报错
                if (messageLocationSender.Error == ErrorCore.ERR_NotFoundActor)
                {
                    return MessageHelper.CreateResponse(iRequest, messageLocationSender.Error);
                }
                
                try
                {
                    return await self.CallInner(messageLocationSender, rpcId, iRequest);
                }
                catch (RpcException)
                {
                    self.Remove(messageLocationSender.Id);
                    throw;
                }
                catch (Exception e)
                {
                    self.Remove(messageLocationSender.Id);
                    throw new Exception($"{iRequest}", e);
                }
            }
        }

        private static async ETTask<IResponse> CallInner(this MessageLocationSenderOneType self, MessageLocationSender messageLocationSender, int rpcId, IRequest iRequest)
        {
            int failTimes = 0;
            long instanceId = messageLocationSender.InstanceId;
            messageLocationSender.LastSendOrRecvTime = TimeInfo.Instance.ServerNow();
            
            Scene root = self.Root();
            
            while (true)
            {
                if (messageLocationSender.ActorId == default)
                {
                    messageLocationSender.ActorId = await root.GetComponent<LocationProxyComponent>().Get(self.LocationType, messageLocationSender.Id);
                    if (messageLocationSender.InstanceId != instanceId)
                    {
                        throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout2, $"{iRequest}");
                    }
                }

                if (messageLocationSender.ActorId == default)
                {
                    messageLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                    return MessageHelper.CreateResponse(iRequest, ErrorCore.ERR_NotFoundActor);
                }
                IResponse response = await root.GetComponent<MessageSender>().Call(messageLocationSender.ActorId, rpcId, iRequest, needException: false);
                
                if (messageLocationSender.InstanceId != instanceId)
                {
                    throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout3, $"{iRequest}");
                }
                
                switch (response.Error)
                {
                    case ErrorCore.ERR_NotFoundActor:
                    {
                        // 如果没找到Actor,重试
                        ++failTimes;
                        if (failTimes > 20)
                        {
                            Log.Debug($"actor send message fail, actorid: {messageLocationSender.Id} {iRequest}");
                            messageLocationSender.Error = ErrorCore.ERR_NotFoundActor;
                            // 这里不能删除actor，要让后面等待发送的消息也返回ERR_NotFoundActor，直到超时删除
                            return response;
                        }

                        // 等待0.5s再发送
                        await root.Fiber.TimerComponent.WaitAsync(500);
                        if (messageLocationSender.InstanceId != instanceId)
                        {
                            throw new RpcException(ErrorCore.ERR_ActorLocationSenderTimeout4, $"{iRequest}");
                        }

                        messageLocationSender.ActorId = default;
                        continue;
                    }
                    case ErrorCore.ERR_MessageTimeout:
                    {
                        throw new RpcException(response.Error, $"{iRequest}");
                    }
                }

                if (ErrorCore.IsRpcNeedThrowException(response.Error))
                {
                    throw new RpcException(response.Error, $"Message: {response.Message} Request: {iRequest}");
                }

                return response;
            }
        }
    }

    [EntitySystemOf(typeof(MessageLocationSenderComponent))]
    [FriendOf(typeof (MessageLocationSenderComponent))]
    public static partial class MessageLocationSenderManagerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MessageLocationSenderComponent self)
        {
            for (int i = 0; i < self.messageLocationSenders.Length; ++i)
            {
                self.messageLocationSenders[i] = self.AddChild<MessageLocationSenderOneType, int>(i);
            }
        }
        
        public static MessageLocationSenderOneType Get(this MessageLocationSenderComponent self, int locationType)
        {
            return self.messageLocationSenders[locationType];
        }
    }
}