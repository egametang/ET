using System;
using Model;

namespace Hotfix
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

				if (message.CollectionName == "")
				{
					message.CollectionName = message.Entitys[0].GetType().Name;
				}

				if (message.NeedCache)
				{
					foreach (Entity entity in message.Entitys)
					{
						dbCacheComponent.AddToCache(entity, message.CollectionName);
					}
				}

				await dbCacheComponent.AddBatch(message.Entitys, message.CollectionName);

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}