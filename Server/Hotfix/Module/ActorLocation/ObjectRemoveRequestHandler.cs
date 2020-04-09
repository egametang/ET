using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectRemoveRequestHandler : AMRpcHandler<ObjectRemoveRequest, ObjectRemoveResponse>
	{
		protected override async ETTask Run(Session session, ObjectRemoveRequest request, ObjectRemoveResponse response, Action reply)
		{
			Game.Scene.GetComponent<LocationComponent>().Remove(request.Key);
			reply();
			await ETTask.CompletedTask;
		}
	}
}