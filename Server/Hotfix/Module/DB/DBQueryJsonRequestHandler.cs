using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryJsonRequestHandler : AMRpcHandler<DBQueryJsonRequest, DBQueryJsonResponse>
	{
		protected override void Run(Session session, DBQueryJsonRequest message, Action<DBQueryJsonResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, DBQueryJsonRequest message, Action<DBQueryJsonResponse> reply)
		{
			DBQueryJsonResponse response = new DBQueryJsonResponse();
			try
			{
				List<ComponentWithId> components = await Game.Scene.GetComponent<DBComponent>().GetJson(message.CollectionName, message.Json);
				response.Components = components;

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}