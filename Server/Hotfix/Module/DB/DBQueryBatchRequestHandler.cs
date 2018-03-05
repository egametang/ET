using System;
using System.Collections.Generic;
using ETModel;

namespace ETHotfix
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
				List<Component> disposers = await dbCacheComponent.GetBatch(message.CollectionName, message.IdList);

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