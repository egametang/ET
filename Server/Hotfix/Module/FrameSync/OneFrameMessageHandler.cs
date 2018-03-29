using System.Threading.Tasks;
using ETModel;

namespace ETHotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class OneFrameMessageHandler: AMActorHandler<Unit, OneFrameMessage>
    {
	    protected override async Task Run(Unit entity, OneFrameMessage message)
	    {
		    Game.Scene.GetComponent<ServerFrameComponent>().Add(message);
		    await Task.CompletedTask;
	    }
    }
}
