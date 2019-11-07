﻿using System;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorMessageSenderComponentAwakeSystem : AwakeSystem<ActorMessageSenderComponent>
    {
        public override void Awake(ActorMessageSenderComponent self)
        {
            ActorMessageSenderComponent.Instance = self;

            self.TimeoutCheckTimer = TimerComponent.Instance.NewRepeatedTimer(10 * 1000, self.Check);
        }
    }
    
    [ObjectSystem]
    public class ActorMessageSenderComponentDestroySystem: DestroySystem<ActorMessageSenderComponent>
    {
        public override void Destroy(ActorMessageSenderComponent self)
        {
            ActorMessageSenderComponent.Instance = null;
            TimerComponent.Instance.Remove(self.TimeoutCheckTimer);
            self.TimeoutCheckTimer = 0;
            self.TimeoutActorMessageSenders.Clear();
        }
    }
    
    public static class ActorMessageSenderComponentSystem
    {
        public static void Check(this ActorMessageSenderComponent self)
        {
            long timeNow = TimeHelper.Now();
            foreach ((int key, ActorMessageSender value) in self.requestCallback)
            {
                if (timeNow < value.CreateTime + ActorMessageSenderComponent.TIMEOUT_TIME)
                {
                    continue;
                }
                self.TimeoutActorMessageSenders.Add(key);
            }

            foreach (int rpcId in self.TimeoutActorMessageSenders)
            {
                ActorMessageSender actorMessageSender = self.requestCallback[rpcId];
                self.requestCallback.Remove(rpcId);
                Log.Error($"actor request timeout: {rpcId}");
                actorMessageSender.Callback.Invoke(new ActorResponse() {Error = ErrorCode.ERR_ActorTimeout});
            }
            
            self.TimeoutActorMessageSenders.Clear();
        }
        
        public static void Send(this ActorMessageSenderComponent self, long actorId, IActorMessage message)
        {
            if (actorId == 0)
            {
                throw new Exception($"actor id is 0: {MongoHelper.ToJson(message)}");
            }
            string address = StartConfigComponent.Instance.GetProcessInnerAddress(IdGenerater.GetProcessId(actorId));
            Session session = NetInnerComponent.Instance.Get(address);
            message.ActorId = actorId;
            session.Send(message);
        }
		
        public static ETTask<IActorResponse> Call(this ActorMessageSenderComponent self, long actorId, IActorRequest message)
        {
            if (actorId == 0)
            {
                throw new Exception($"actor id is 0: {MongoHelper.ToJson(message)}");
            }
            
            string address = StartConfigComponent.Instance.GetProcessInnerAddress(IdGenerater.GetProcessId(actorId));
            Session session = NetInnerComponent.Instance.Get(address);
            message.ActorId = actorId & IdGenerater.HeadMask | IdGenerater.Head;
            message.RpcId = ++self.RpcId;
			
            var tcs = new ETTaskCompletionSource<IActorResponse>();
            self.requestCallback.Add(message.RpcId, new ActorMessageSender((response) =>
            {
                if (ErrorCode.IsRpcNeedThrowException(response.Error))
                {
                    tcs.SetException(new Exception($"Rpc error: {MongoHelper.ToJson(response)}"));
                    return;
                }

                
                tcs.SetResult(response);
            }));
            session.Send(message);
            return tcs.Task;
        }
		
        public static ETTask<IActorResponse> CallWithoutException(this ActorMessageSenderComponent self, long actorId,  IActorRequest message)
        {
            if (actorId == 0)
            {
                throw new Exception($"actor id is 0: {MongoHelper.ToJson(message)}");
            }
            
            string address = StartConfigComponent.Instance.GetProcessInnerAddress(IdGenerater.GetProcessId(actorId));
            Session session = NetInnerComponent.Instance.Get(address);
            message.ActorId = actorId & IdGenerater.HeadMask | IdGenerater.Head;
            message.RpcId = ++self.RpcId;
			
            var tcs = new ETTaskCompletionSource<IActorResponse>();
            self.requestCallback.Add(message.RpcId, new ActorMessageSender((response) =>
            {
                tcs.SetResult(response);
            }));
            session.Send(message);
            return tcs.Task;
        }
		
        public static void RunMessage(this ActorMessageSenderComponent self, IActorResponse response)
        {
            ActorMessageSender actorMessageSender;
            if (!self.requestCallback.TryGetValue(response.RpcId, out actorMessageSender))
            {
                Log.Error($"not found rpc, maybe request timeout, response message: {StringHelper.MessageToStr(response)}");
                return;
            }
            self.requestCallback.Remove(response.RpcId);
            
            actorMessageSender.Callback(response);
        }
    }
}