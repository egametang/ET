using System;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler]
	public class ObjectGetRequestHandler : AMActorRpcHandler<Scene, ObjectGetRequest, ObjectGetResponse>
	{
		protected override async ETTask Run(Scene scene, ObjectGetRequest request, ObjectGetResponse response, Action reply)
		{
			long instanceId = await scene.GetComponent<LocationComponent>().Get(request.Key);
			if (instanceId == 0)
			{
				response.Error = ErrorCode.ERR_ActorLocationNotFound;
			}
			response.InstanceId = instanceId;
			reply();
		}
	}
}