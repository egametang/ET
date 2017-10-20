using System;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryRequestHandler : AMRpcHandler<DBQueryRequest, DBQueryResponse>
	{
		protected override async void Run(Session session, DBQueryRequest message, Action<DBQueryResponse> reply)
		{
			DBQueryResponse response = new DBQueryResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				Entity entity = await dbCacheComponent.Get(message.CollectionName, message.Id);

				response.Entity = entity;

				if (message.NeedCache && entity != null)
				{
					dbCacheComponent.AddToCache(entity, message.CollectionName);
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