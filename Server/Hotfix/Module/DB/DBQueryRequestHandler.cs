using System;
using ETModel;

namespace ETHotfix
{
    /// <summary>
    ///数据添加到数据库缓存字
    /// </summary>
	[MessageHandler(AppType.DB)]
	public class DBQueryRequestHandler : AMRpcHandler<DBQueryRequest, DBQueryResponse>
	{
		protected override async void Run(Session session, DBQueryRequest message, Action<DBQueryResponse> reply)
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