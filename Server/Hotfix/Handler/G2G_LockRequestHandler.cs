using System;
using ETModel;

namespace ETHotfix
{
	[MessageHandler(AppType.Gate)]
	public class G2G_LockRequestHandler : AMRpcHandler<G2G_LockRequest, G2G_LockResponse>
	{
		protected override void Run(Session session, G2G_LockRequest message, Action<G2G_LockResponse> reply)
		{
			RunAsync(session, message, reply).NoAwait();
		}
		
		protected async ETVoid RunAsync(Session session, G2G_LockRequest message, Action<G2G_LockResponse> reply)
		{
			G2G_LockResponse response = new G2G_LockResponse();
			try
			{
				Unit unit = Game.Scene.GetComponent<UnitComponent>().Get(message.Id);
				if (unit == null)
				{
					response.Error = ErrorCode.ERR_NotFoundUnit;
					reply(response);
					return;
				}

				await unit.GetComponent<MasterComponent>().Lock(NetworkHelper.ToIPEndPoint(message.Address));

				reply(response);
			}
			catch (Exception e)
			{
				ReplyError(response, e, reply);
			}
		}
	}
}