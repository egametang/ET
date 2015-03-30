using System;
using Common.Network;
using Model;

namespace Controller
{
	[Event(EventType.GateRecvClientMessage, ServerType.Gate)]
	public class GateRecvClientMessageEvent : IEventSync
	{
		public void Run(Env env)
		{
			byte[] message = env.Get<byte[]>(EnvKey.Message);
			AChannel channel = env.Get<AChannel>(EnvKey.Channel);

			// 进行消息分发
			ChannelUnitInfoComponent channelUnitInfoComponent = channel.GetComponent<ChannelUnitInfoComponent>();
			byte[] idBuffer = channelUnitInfoComponent.UnitId.ToByteArray();
			byte[] buffer = new byte[message.Length + 12];
			Array.Copy(message, 0, buffer, 0, 4);
			Array.Copy(idBuffer, 0, buffer, 4, idBuffer.Length);
			Array.Copy(message, 4, buffer, 4 + 12, message.Length - 4);
			string address = AddressHelper.GetAddressByServerName(channelUnitInfoComponent.ServerName);
			World.Instance.GetComponent<NetworkComponent>().SendAsync(address, buffer);
		}
	}
}
