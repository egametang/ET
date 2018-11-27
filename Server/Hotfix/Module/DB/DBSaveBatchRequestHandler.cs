using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBSaveBatchRequestHandler : AMRpcHandler<DBSaveBatchRequest, DBSaveBatchResponse>
	{
		protected override void Run(Session session, DBSaveBatchRequest message, Action<DBSaveBatchResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, DBSaveBatchRequest message, Action<DBSaveBatchResponse> reply)
		{
			DBSaveBatchResponse response = new DBSaveBatchResponse();
			try
			{
				DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

				if (string.IsNullOrEmpty(message.CollectionName))
				{
					message.CollectionName = message.Components[0].GetType().Name;
				}

				await dbComponent.AddBatch(message.Components, message.CollectionName);

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}
