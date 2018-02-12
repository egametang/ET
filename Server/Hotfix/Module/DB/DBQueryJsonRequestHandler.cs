using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryJsonRequestHandler : AMRpcHandler<DBQueryJsonRequest, DBQueryJsonResponse>
	{
		protected override async void Run(Session session, DBQueryJsonRequest message, Action<DBQueryJsonResponse> reply)
		{
			DBQueryJsonResponse response = new DBQueryJsonResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				List<Component> disposers = await dbCacheComponent.GetJson(message.CollectionName, message.Json);

				response.Disposers = disposers;

				if (message.NeedCache)
				{
					foreach (Component disposer in disposers)
					{
						dbCacheComponent.AddToCache(disposer, message.CollectionName);
					}
				}

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}