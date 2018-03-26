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
					message.CollectionName = message.Components[0].GetType().Name;
				}

				if (message.NeedCache)
				{
					foreach (ComponentWithId component in message.Components)
					{
						dbCacheComponent.AddToCache(component, message.CollectionName);
					}
				}

				await dbCacheComponent.AddBatch(message.Components, message.CollectionName);

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}
