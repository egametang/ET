using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectGetRequestHandler : AMRpcHandler<ObjectGetRequest, ObjectGetResponse>
	{
		protected override async void Run(Session session, ObjectGetRequest message, Action<ObjectGetResponse> reply)
		{
			ObjectGetResponse response = new ObjectGetResponse();
			try
			{
				int appId = await Game.Scene.GetComponent<LocationComponent>().GetAsync(message.Key);
				if (appId == 0)
				{
					response.Error = ErrorCode.ERR_ActorLocationNotFound;
				}
				response.AppId = appId;
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}