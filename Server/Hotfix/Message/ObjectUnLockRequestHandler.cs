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
				Game.Scene.GetComponent<LocationComponent>().UnLock(message.Key);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}