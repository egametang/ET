using System;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectAddRequestHandler : AMRpcHandler<ObjectAddRequest, ObjectAddResponse>
	{
		protected override void Run(Session session, ObjectAddRequest message, Action<ObjectAddResponse> reply)
		{
			ObjectAddResponse response = new ObjectAddResponse();
			try
			{
				Game.Scene.GetComponent<LocationComponent>().Add(message.Key, session.RemoteAddress);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}