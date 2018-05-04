using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectUnLockRequestHandler : AMRpcHandler<ObjectUnLockRequest, ObjectUnLockResponse>
	{
		protected override void Run(Session session, ObjectUnLockRequest message, Action<ObjectUnLockResponse> reply)
		{
			ObjectUnLockResponse response = new ObjectUnLockResponse();
			try
			{
				Game.Scene.GetComponent<LocationComponent>().UnLockAndUpdate(message.Key, message.OldInstanceId, message.InstanceId);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}