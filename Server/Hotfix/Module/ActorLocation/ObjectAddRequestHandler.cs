using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler]
	public class ObjectAddRequestHandler : AMActorRpcHandler<Scene, ObjectAddRequest, ObjectAddResponse>
	{
		protected override async ETTask Run(Scene scene, ObjectAddRequest request, ObjectAddResponse response, Action reply)
		{
			await scene.GetComponent<LocationComponent>().Add(request.Key, request.InstanceId);
			reply();
		}
	}
}