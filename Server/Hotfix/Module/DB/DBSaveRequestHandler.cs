using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBSaveRequestHandler : AMRpcHandler<DBSaveRequest, DBSaveResponse>
	{
		protected override void Run(Session session, DBSaveRequest message, Action<DBSaveResponse> reply)
		{
			RunAsync(session, message, reply).NoAwait();
		}
		
		protected async ETVoid RunAsync(Session session, DBSaveRequest message, Action<DBSaveResponse> reply)
		{
			DBSaveResponse response = new DBSaveResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				if (string.IsNullOrEmpty(message.CollectionName))
				{
					message.CollectionName = message.Component.GetType().Name;
				}

				if (message.NeedCache)
				{
					dbCacheComponent.AddToCache(message.Component, message.CollectionName);
				}
				await dbCacheComponent.Add(message.Component, message.CollectionName);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}