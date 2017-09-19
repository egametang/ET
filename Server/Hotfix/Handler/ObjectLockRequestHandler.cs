﻿using System;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectLockRequestHandler : AMRpcHandler<ObjectLockRequest, ObjectLockResponse>
	{
		protected override void Run(Session session, ObjectLockRequest message, Action<ObjectLockResponse> reply)
		{
			ObjectLockResponse response = new ObjectLockResponse();
			try
			{
				Game.Scene.GetComponent<LocationComponent>().LockAsync(message.Key, message.LockAppId, message.Time);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}