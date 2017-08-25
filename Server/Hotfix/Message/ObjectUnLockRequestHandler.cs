using System;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectUnLockRequestHandler : AMRpcHandler<ObjectUnLockRequest, ObjectUnLockResponse>
	{
		protected override void Run(Session session, ObjectUnLockRequest message, Action<ObjectUnLockResponse> reply)
		{
			ObjectUnLockResponse response = new ObjectUnLockResponse();
			try
			{
				Game.Scene.GetComponent<LocationComponent>().UpdateAndUnLock(message.Key, message.AppId, message.Value);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}