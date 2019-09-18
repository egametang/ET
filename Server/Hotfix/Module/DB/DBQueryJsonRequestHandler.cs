using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryJsonRequestHandler : AMRpcHandler<DBQueryJsonRequest, DBQueryJsonResponse>
	{
		protected override async ETTask Run(Session session, DBQueryJsonRequest request, DBQueryJsonResponse response, Action reply)
		{
			List<ComponentWithId> components = await Game.Scene.GetComponent<DBComponent>().GetJson(request.CollectionName, request.Json);
			response.Components = components;

			reply();
			await ETTask.CompletedTask;
		}
	}
}