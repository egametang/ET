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
			this.Address = await this.Parent.GetComponent<LocationProxyComponent>().Get(this.Id);
			
			Session session = Game.Scene.GetComponent<NetInnerComponent>().Get(this.Address);
			Response response = await session.Call<Request, Response>(request);
			return response;
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