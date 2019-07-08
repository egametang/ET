using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBSaveBatchRequestHandler : AMRpcHandler<DBSaveBatchRequest, DBSaveBatchResponse>
	{
		protected override async ETTask Run(Session session, DBSaveBatchRequest request, DBSaveBatchResponse response, Action reply)
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();

			if (string.IsNullOrEmpty(request.CollectionName))
			{
				request.CollectionName = request.Components[0].GetType().Name;
			}

			await dbComponent.AddBatch(request.Components, request.CollectionName);

			reply();
		}
	}
}
