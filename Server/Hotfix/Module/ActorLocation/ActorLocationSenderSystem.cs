using System;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorLocationSenderAwakeSystem : AwakeSystem<ActorLocationSender>
    {
        public override void Awake(ActorLocationSender self)
        {
            self.LastRecvTime = TimeHelper.Now();
            self.FailTimes = 0;
            self.ActorId = 0;       
        }
        
    }
	
    [ObjectSystem]
    public class ActorLocationSenderDestroySystem : DestroySystem<ActorLocationSender>
    {
        public override void Destroy(ActorLocationSender self)
        {
            self.Id = 0;
            self.LastRecvTime = 0;
            self.ActorId = 0;
            self.FailTimes = 0;
        }
    }
    
    public static class ActorLocationSenderSystem
    {
		public static async ETTask GetActorId(this ActorLocationSender self)
		{
			using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ActorLocationSender, self.Id))
			{
				self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
			}
		}

        private static async ETTask<IActorResponse> Run(this ActorLocationSender self, IActorRequest iActorRequest)
        {
            long instanceId = self.InstanceId;
            using (await CoroutineLockComponent.Instance.Wait(CoroutineLockType.ActorLocationSender, self.Id))
            {
                if (self.InstanceId != instanceId)
                {
                    throw new RpcException(ErrorCode.ERR_ActorRemove, $"{MongoHelper.ToJson(iActorRequest)}");
                }

                return await self.RunInner(iActorRequest);
            }
        }

        private static async ETTask<IActorResponse> RunInner(this ActorLocationSender self, IActorRequest iActorRequest)
        {
            try
            {
                if (self.ActorId == 0)
                {
                    Log.Info($"actor send message fail, actorid: {self.Id}");
                    self.Dispose();
                    return new ActorResponse() { Error = ErrorCode.ERR_ActorNotOnline };
                }

                self.LastRecvTime = TimeHelper.Now();
                ActorMessageSender actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.ActorId);
                IActorResponse response = await actorMessageSender.CallWithoutException(iActorRequest);

                switch (response.Error)
                {
                    case ErrorCode.ERR_NotFoundActor:
                        {
                            // 如果没找到Actor,重试
                            ++self.FailTimes;
                            if (self.FailTimes > ActorLocationSender.MaxFailTimes)
                            {
                                Log.Info($"actor send message fail, actorid: {self.Id}");
                                self.Dispose();
                                return response;
                            }

                            // 等待0.5s再发送
                            long instanceId = self.InstanceId;
                            await TimerComponent.Instance.WaitAsync(500);
                            if (self.InstanceId != instanceId)
                            {
                                throw new RpcException(ErrorCode.ERR_ActorRemove, $"{MongoHelper.ToJson(iActorRequest)}");
                            }
                            self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
                            return await self.RunInner(iActorRequest);
                        }
                }

                self.LastRecvTime = TimeHelper.Now();
                self.FailTimes = 0;

                return response;
            }
            catch (Exception e)
            {
                self.Dispose();
                throw new Exception($"{MongoHelper.ToJson(iActorRequest)}\n{e}");
            }
        }

        public static async ETVoid Send(this ActorLocationSender self, IActorLocationMessage request)
	    {
		    if (request == null)
		    {
			    throw new Exception($"actor location send message is null");
		    }
		    
			await Run(self, request);
	    }

		public static async ETTask<IActorLocationResponse> Call(this ActorLocationSender self, IActorLocationRequest request)
		{
			if (request == null)
			{
				throw new Exception($"actor location call message is null");
			}
			
			return await Run(self, request) as IActorLocationResponse;
		}
    }
}