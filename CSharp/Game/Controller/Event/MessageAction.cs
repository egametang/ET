using System;
using System.Threading.Tasks;
using Common.Network;
using Model;

namespace Controller
{
	[Event(EventType.MessageAction)]
	public class MessageAction: IEventAsync
	{
		public async Task RunAsync(Env env)
		{
			AChannel channel = env.Get<AChannel>(EnvKey.Channel);
			ChannelUnitInfoComponent channelUnitInfoComponent =
					channel.GetComponent<ChannelUnitInfoComponent>();
			if (channelUnitInfoComponent != null)
			{
				Unit unit = World.Instance.GetComponent<UnitComponent>().Get(channelUnitInfoComponent.PlayerId);
				if (unit == null)
				{
					return;
				}
				unit.GetComponent<ActorComponent>().Add(env);
				return;
			}

			var message = env.Get<byte[]>(EnvKey.Message);
			int opcode = BitConverter.ToUInt16(message, 0);
			await World.Instance.GetComponent<EventComponent<MessageAttribute>>().RunAsync(opcode, env);
		}
	}
}