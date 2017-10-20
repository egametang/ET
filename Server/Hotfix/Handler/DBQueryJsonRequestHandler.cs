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
				List<Entity> entitys = await dbCacheComponent.GetJson(message.CollectionName, message.Json);

				response.Entitys = entitys;

				if (message.NeedCache)
				{
					foreach (Entity entity in entitys)
					{
						dbCacheComponent.AddToCache(entity, message.CollectionName);
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