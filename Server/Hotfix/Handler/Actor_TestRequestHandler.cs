using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class Actor_TestRequestHandler : AMActorLocationRpcHandler<Unit, Actor_TestRequest, Actor_TestResponse>
	{
		protected override async Task Run(Unit unit, Actor_TestRequest message, Action<Actor_TestResponse> reply)
		{
			await Task.CompletedTask;
			reply(new Actor_TestResponse() {Response = "response actor rpc"});
		}
	}
}