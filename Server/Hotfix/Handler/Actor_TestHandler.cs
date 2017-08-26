using System.Threading.Tasks;
using Model;

namespace Hotfix
{
	[ActorMessageHandler(AppType.Map)]
	public class Actor_TestHandler : AMActorHandler<Unit, Actor_Test>
	{
		protected override async Task<bool> Run(Unit unit, Actor_Test message)
		{
			Log.Info(message.Info);
			return true;
		}
	}
}