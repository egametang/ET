using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectGetRequestHandler : AMRpcHandler<ObjectGetRequest, ObjectGetResponse>
	{
		protected override void Run(Session session, ObjectGetRequest message, Action<ObjectGetResponse> reply)
		{
			RunAsync(session, message, reply).NoAwait();
		}
		
		private async ETVoid RunAsync(Session session, ObjectGetRequest message, Action<ObjectGetResponse> reply)
		{
			ObjectGetResponse response = new ObjectGetResponse();
			try
			{
				long instanceId = await Game.Scene.GetComponent<LocationComponent>().GetAsync(message.Key);
				if (instanceId == 0)
				{
					response.Error = ErrorCode.ERR_ActorLocationNotFound;
				}
				response.InstanceId = instanceId;
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}