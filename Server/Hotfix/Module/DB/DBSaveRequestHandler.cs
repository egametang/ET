using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.DB)]
	public class DBSaveRequestHandler : AMRpcHandler<DBSaveRequest, DBSaveResponse>
	{
		protected override void Run(Session session, DBSaveRequest message, Action<DBSaveResponse> reply)
		{
			RunAsync(session, message, reply).Coroutine();
		}
		
		protected async ETVoid RunAsync(Session session, DBSaveRequest message, Action<DBSaveResponse> reply)
		{
			DBSaveResponse response = new DBSaveResponse();
			try
			{
				DBComponent dbComponent = Game.Scene.GetComponent<DBComponent>();
				if (string.IsNullOrEmpty(message.CollectionName))
				{
					message.CollectionName = message.Component.GetType().Name;
				}

				await dbComponent.Add(message.Component, message.CollectionName);
				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}