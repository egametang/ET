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
				Game.Scene.GetComponent<LocationComponent>().UpdateAndUnLock(message.Key, message.UnLockAppId, message.AppId);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}