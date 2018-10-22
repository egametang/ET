using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
    [ObjectSystem]
    public class ActorLocationSenderAwakeSystem : AwakeSystem<ActorLocationSender, long>
    {
        public override void Awake(ActorLocationSender self, long id)
        {
            self.LastSendTime = TimeHelper.Now();
	        self.Id = id;
            self.Tcs = null;
            self.FailTimes = 0;
            self.ActorId = 0;
        }
    }

    [ObjectSystem]
    public class ActorLocationSenderStartSystem : StartSystem<ActorLocationSender>
    {
	    public override void Start(ActorLocationSender self)
	    {
		    StartAsync(self).NoAwait();
	    }
	    
        public async ETVoid StartAsync(ActorLocationSender self)
        {
            self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);

            self.Address = StartConfigComponent.Instance
                    .Get(IdGenerater.GetAppIdFromId(self.ActorId))
                    .GetComponent<InnerConfig>().IPEndPoint;

            self.UpdateAsync().NoAwait();
        }
    }
	
    [ObjectSystem]
    public class ActorLocationSenderDestroySystem : DestroySystem<ActorLocationSender>
    {
        public override void Destroy(ActorLocationSender self)
        {
	        self.RunError(ErrorCode.ERR_ActorRemove);
	        
            self.Id = 0;
            self.LastSendTime = 0;
            self.Address = null;
            self.ActorId = 0;
            self.FailTimes = 0;
            self.Tcs = null;
        }
    }
    
    public static class ActorLocationSenderHelper
    {
    	private static void Add(this ActorLocationSender self, ActorTask task)
		{
			if (self.IsDisposed)
			{
				throw new Exception("ActorLocationSender Disposed! dont hold ActorMessageSender");
			}

			self.WaitingTasks.Enqueue(task);
			// failtimes > 0表示正在重试，这时候不能加到正在发送队列
			if (self.FailTimes == 0)
			{
				self.AllowGet();
			}
		}

	    public static void RunError(this ActorLocationSender self, int errorCode)
	    {
		    while (self.WaitingTasks.Count > 0)
		    {
			    ActorTask actorTask = self.WaitingTasks.Dequeue();
			    actorTask.Tcs?.SetException(new RpcException(errorCode, ""));
		    }
		    self.WaitingTasks.Clear();
	    }

		private static void AllowGet(this ActorLocationSender self)
		{
			if (self.Tcs == null || self.WaitingTasks.Count <= 0)
			{
				return;
			}

			ActorTask task = self.WaitingTasks.Peek();

			var t = self.Tcs;
			self.Tcs = null;
			t.SetResult(task);
		}

		private static ETTask<ActorTask> GetAsync(this ActorLocationSender self)
		{
			if (self.WaitingTasks.Count > 0)
			{
				ActorTask task = self.WaitingTasks.Peek();
				return ETTask.FromResult(task);
			}

			self.Tcs = new ETTaskCompletionSource<ActorTask>();
			return self.Tcs.Task;
		}

		public static async ETVoid UpdateAsync(this ActorLocationSender self)
		{
			try
			{
				long instanceId = self.InstanceId;
				while (true)
				{
					if (self.InstanceId != instanceId)
					{
						return;
					}
					ActorTask actorTask = await self.GetAsync();
					
					if (self.InstanceId != instanceId)
					{
						return;
					}
					if (actorTask.ActorRequest == null)
					{
						return;
					}

					await self.RunTask(actorTask);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private static async ETTask RunTask(this ActorLocationSender self, ActorTask task)
		{
			ActorMessageSender actorMessageSender = Game.Scene.GetComponent<ActorMessageSenderComponent>().Get(self.ActorId);
			IActorResponse response = await actorMessageSender.Call(task.ActorRequest);
			
			// 发送成功
			switch (response.Error)
			{
				case ErrorCode.ERR_NotFoundActor:
					// 如果没找到Actor,重试
					++self.FailTimes;

					// 失败MaxFailTimes次则清空actor发送队列，返回失败
					if (self.FailTimes > ActorLocationSender.MaxFailTimes)
					{
						// 失败直接删除actorproxy
						Log.Info($"actor send message fail, actorid: {self.Id}");
						self.RunError(response.Error);
						self.GetParent<ActorLocationSenderComponent>().Remove(self.Id);
						return;
					}

					// 等待0.5s再发送
					await Game.Scene.GetComponent<TimerComponent>().WaitAsync(500);
					self.ActorId = await Game.Scene.GetComponent<LocationProxyComponent>().Get(self.Id);
					self.Address = StartConfigComponent.Instance
							.Get(IdGenerater.GetAppIdFromId(self.ActorId))
							.GetComponent<InnerConfig>().IPEndPoint;
					self.AllowGet();
					return;
				
				case ErrorCode.ERR_ActorNoMailBoxComponent:
					self.RunError(response.Error);
					self.GetParent<ActorLocationSenderComponent>().Remove(self.Id);
					return;
				
				default:
					self.LastSendTime = TimeHelper.Now();
					self.FailTimes = 0;
					self.WaitingTasks.Dequeue();
					
					if (task.Tcs == null)
					{
						return;
					}
					
					IActorLocationResponse actorLocationResponse = response as IActorLocationResponse;
					if (actorLocationResponse == null)
					{
						task.Tcs.SetException(new Exception($"actor location respose is not IActorLocationResponse, but is: {response.GetType().Name}"));
					}
					task.Tcs.SetResult(actorLocationResponse);
					return;
			}
		}

	    public static void Send(this ActorLocationSender self, IActorLocationMessage request)
	    {
		    if (request == null)
		    {
			    throw new Exception($"actor location send message is null");
		    }
		    ActorTask task = new ActorTask(request);
		    self.Add(task);
	    }

		public static ETTask<IActorLocationResponse> Call(this ActorLocationSender self, IActorLocationRequest request)
		{
			if (request == null)
			{
				throw new Exception($"actor location call message is null");
			}
			ETTaskCompletionSource<IActorLocationResponse> tcs = new ETTaskCompletionSource<IActorLocationResponse>();
			ActorTask task = new ActorTask(request, tcs);
			self.Add(task);
			return task.Tcs.Task;
		}
    }
}