using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryRequestHandler : AMRpcHandler<DBQueryRequest, DBQueryResponse>
	{
		protected override void Run(Session session, DBQueryRequest message, Action<DBQueryResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, DBQueryRequest message, Action<DBQueryResponse> reply)
		{
			DBQueryResponse response = new DBQueryResponse();
			try
			{
				ComponentWithId component = await Game.Scene.GetComponent<DBComponent>().Get(message.CollectionName, message.Id);

				response.Component = component;

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}