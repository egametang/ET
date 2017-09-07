using System;
using System.Collections.Generic;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryBatchRequestHandler : AMRpcHandler<DBQueryBatchRequest, DBQueryBatchResponse>
	{
		protected override async void Run(Session session, DBQueryBatchRequest message, Action<DBQueryBatchResponse> reply)
		{
			DBQueryBatchResponse response = new DBQueryBatchResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				List<Entity> entitys = await dbCacheComponent.GetBatch(message.CollectionName, message.IdList);

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