using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectAddRequestHandler : AMRpcHandler<ObjectAddRequest, ObjectAddResponse>
	{
		protected override async ETTask Run(Session session, ObjectAddRequest request, ObjectAddResponse response, Action reply)
		{
			Game.Scene.GetComponent<LocationComponent>().Add(request.Key, request.InstanceId);
			reply();
		}
	}
}