using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryBatchRequestHandler : AMRpcHandler<DBQueryBatchRequest, DBQueryBatchResponse>
	{
		protected override void Run(Session session, DBQueryBatchRequest message, Action<DBQueryBatchResponse> reply)
		{
			RunAsync(session, message, reply).NoAwait();
		}
		
		protected async ETVoid RunAsync(Session session, DBQueryBatchRequest message, Action<DBQueryBatchResponse> reply)
		{
			DBQueryBatchResponse response = new DBQueryBatchResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				List<ComponentWithId> components = await dbCacheComponent.GetBatch(message.CollectionName, message.IdList);

				response.Components = components;

				if (message.NeedCache)
				{
					foreach (ComponentWithId component in components)
					{
						dbCacheComponent.AddToCache(component, message.CollectionName);
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