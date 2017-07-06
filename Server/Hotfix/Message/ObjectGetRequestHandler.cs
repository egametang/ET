using System;
using Model;

namespace Hotfix
{
	[MessageHandler(AppType.Location)]
	public class ObjectGetRequestHandler : AMRpcHandler<ObjectGetRequest, ObjectGetResponse>
	{
		protected override async void Run(Session session, ObjectGetRequest message, Action<ObjectGetResponse> reply)
		{
			ObjectGetResponse response = new ObjectGetResponse();
			try
			{
				string location = await Game.Scene.GetComponent<LocationComponent>().GetAsync(message.Key);
				response.Location = location;
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}