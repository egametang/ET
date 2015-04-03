using Common.Network;
using Model;

namespace Controller
{
	[Event(EventType.GateRecvClientMessage, ServerType.Gate)]
	public class GateRecvClientMessageEvent: IEventSync
	{
		public void Run(Env env)
		{
			byte[] messageBytes = env.Get<byte[]>(EnvKey.MessageBytes);
			AChannel channel = env.Get<AChannel>(EnvKey.Channel);

			// 进行消息分发
			ChannelUnitInfoComponent channelUnitInfoComponent =
					channel.GetComponent<ChannelUnitInfoComponent>();
			byte[] bytes = MessageParseHelper.ClientToGateMessageChangeToLogicMessage(messageBytes,
					channelUnitInfoComponent.UnitId);
			string address = AddressHelper.GetAddressByServerName(channelUnitInfoComponent.ServerName);
			World.Instance.GetComponent<NetworkComponent>().SendAsync(address, bytes);
		}
	}
}