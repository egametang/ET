using System;
using System.Threading.Tasks;

namespace Model
{
	public sealed class ActorProxy : Entity
	{
		public string Address;
		
		public ActorProxy(long id): base(id)
		{
		}

		public void Send<Message>(Message message) where Message : AActorMessage
		{
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.Address);
			session.Send(message);
		}

		public async Task<Response> Call<Request, Response>(Request request) where Request : AActorRequest where Response: AActorResponse
		{
			try
			{
				Response response = null;
				if (this.Address == "")
				{
					this.Address = await this.Parent.GetComponent<LocationProxyComponent>().Get(this.Id);
				}
				response = await OnceCall<Request, Response>(0, request);
				return response;
			}
			catch (RpcException e)
			{
				Console.WriteLine(e);
				throw;
			}
		}

		public async Task<Response> OnceCall<Request, Response>(int retryTime, Request request) where Request : AActorRequest where Response : AActorResponse
		{
			Response response = null;
			if (retryTime > 0)
			{
				await this.Parent.GetComponent<TimerComponent>().WaitAsync(retryTime * 500);
				this.Address = await this.Parent.GetComponent<LocationProxyComponent>().Get(this.Id);
			}
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.Address);
			response = await session.Call<Request, Response>(request);

			if (response.Error == ErrorCode.ERR_Success)
			{
				return response;
			}

			if (retryTime >= 3)
			{
				throw new RpcException(response.Error, response.Message);
			}

			if (response.Error == ErrorCode.ERR_NotFoundActor)
			{
				response = await OnceCall<Request, Response>(++retryTime, request);
			}

			throw new RpcException(response.Error, response.Message);
		}

		public override void Dispose()
		{
			if (this.Id == 0)
			{
				return;
			}

			base.Dispose();
		}
	}
}