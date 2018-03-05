using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBSaveBatchRequestHandler : AMRpcHandler<DBSaveBatchRequest, DBSaveBatchResponse>
	{
		protected override async void Run(Session session, DBSaveBatchRequest message, Action<DBSaveBatchResponse> reply)
		{
			DBSaveBatchResponse response = new DBSaveBatchResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();

				if (string.IsNullOrEmpty(message.CollectionName))
				{
					message.CollectionName = message.Disposers[0].GetType().Name;
				}

				if (message.NeedCache)
				{
					foreach (Component disposer in message.Disposers)
					{
						dbCacheComponent.AddToCache(disposer, message.CollectionName);
					}
				}

				await dbCacheComponent.AddBatch(message.Disposers, message.CollectionName);

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}
