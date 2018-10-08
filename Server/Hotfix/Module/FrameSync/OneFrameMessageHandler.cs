using System;
using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class OneFrameMessageHandler: AMActorLocationHandler<Unit, OneFrameMessage>
    {
	    protected override void Run(Unit unit, OneFrameMessage message)
	    {
			Game.Scene.GetComponent<ServerFrameComponent>().Add(message);
	    }
    }
}
