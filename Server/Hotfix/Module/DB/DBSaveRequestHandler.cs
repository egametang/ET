using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBSaveRequestHandler : AMRpcHandler<DBSaveRequest, DBSaveResponse>
	{
		protected override async void Run(Session session, DBSaveRequest message, Action<DBSaveResponse> reply)
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