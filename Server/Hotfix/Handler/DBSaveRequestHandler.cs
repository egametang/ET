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
				if (string.IsNullOrEmpty(message.CollectionName))
				{
					message.CollectionName = message.Disposer.GetType().Name;
				}

				if (message.NeedCache)
				{
					dbCacheComponent.AddToCache(message.Disposer, message.CollectionName);
				}
				await dbCacheComponent.Add(message.Disposer, message.CollectionName);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}