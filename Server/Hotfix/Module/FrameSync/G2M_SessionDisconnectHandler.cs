using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class G2M_SessionDisconnectHandler : AMActorRpcHandler<Unit, G2M_SessionDisconnect, ActorResponse>
	{
		protected override async Task Run(Unit unit, G2M_SessionDisconnect message, Action<ActorResponse> reply)
		{
			ActorResponse actorResponse = new ActorResponse();
			try
			{
				unit.GetComponent<UnitGateComponent>().IsDisconnect = true;
				reply(actorResponse);
			}
			catch (Exception e)
			{
				ReplyError(actorResponse, e, reply);
			}
			
			await Task.CompletedTask;
		}
	}
}