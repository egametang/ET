using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryRequestHandler : AMRpcHandler<DBQueryRequest, DBQueryResponse>
	{
		protected override void Run(Session session, DBQueryRequest message, Action<DBQueryResponse> reply)
		{
			RunAsync(session, message, reply).NoAwait();
		}
		
		protected async ETVoid RunAsync(Session session, DBQueryRequest message, Action<DBQueryResponse> reply)
		{
			DBQueryResponse response = new DBQueryResponse();
			try
			{
				DBCacheComponent dbCacheComponent = Game.Scene.GetComponent<DBCacheComponent>();
				ComponentWithId component = await dbCacheComponent.Get(message.CollectionName, message.Id);

				response.Component = component;

				if (message.NeedCache && component != null)
				{
					dbCacheComponent.AddToCache(component, message.CollectionName);
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