using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectLockRequestHandler : AMRpcHandler<ObjectLockRequest, ObjectLockResponse>
	{
		protected override async ETTask Run(Session session, ObjectLockRequest request, ObjectLockResponse response, Action reply)
		{
			Game.Scene.GetComponent<LocationComponent>().Lock(request.Key, request.InstanceId, request.Time).Coroutine();
			reply();
		}
	}
}