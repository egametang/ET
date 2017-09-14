using System;
using Model;

namespace Hotfix
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
				if (message.CollectionName == "")
				{
					message.CollectionName = message.Entity.GetType().Name;
				}

				if (message.NeedCache)
				{
					dbCacheComponent.AddToCache(message.Entity, message.CollectionName);
				}
				await dbCacheComponent.Add(message.Entity, message.CollectionName);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}