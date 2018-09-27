using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class OneFrameMessageHandler: AMActorRpcHandler<Unit, OneFrameMessage, ActorResponse>
    {
	    protected override async Task Run(Unit unit, OneFrameMessage message, Action<ActorResponse> reply)
	    {
		    ActorResponse actorResponse = new ActorResponse();
		    try
		    {
			    Game.Scene.GetComponent<ServerFrameComponent>().Add(message);
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
