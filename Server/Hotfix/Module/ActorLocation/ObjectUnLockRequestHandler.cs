using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectUnLockRequestHandler : AMRpcHandler<ObjectUnLockRequest, ObjectUnLockResponse>
	{
		protected override async ETTask Run(Session session, ObjectUnLockRequest request, ObjectUnLockResponse response, Action reply)
		{
			Game.Scene.GetComponent<LocationComponent>().UnLockAndUpdate(request.Key, request.OldInstanceId, request.InstanceId);
			reply();
			await ETTask.CompletedTask;
		}
	}
}