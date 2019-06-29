using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryBatchRequestHandler : AMRpcHandler<DBQueryBatchRequest, DBQueryBatchResponse>
	{
		protected override void Run(Session session, DBQueryBatchRequest message, Action<DBQueryBatchResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, DBQueryBatchRequest message, Action<DBQueryBatchResponse> reply)
		{
			DBQueryBatchResponse response = new DBQueryBatchResponse();
			try
			{
				DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
				response.Components = await dbComponent.GetBatch(message.CollectionName, message.IdList);

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}