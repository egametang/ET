using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBQueryBatchRequestHandler : AMRpcHandler<DBQueryBatchRequest, DBQueryBatchResponse>
	{
		protected override async ETTask Run(Session session, DBQueryBatchRequest request, DBQueryBatchResponse response, Action reply)
		{
			DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
			response.Components = await dbComponent.GetBatch(request.CollectionName, request.IdList);

			reply();
		}
	}
}