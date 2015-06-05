using System;
using System.Collections.Generic;
using Common.Base;
using Common.Network;
using MongoDB.Bson;
using TNet;
using UNet;

namespace Model
{
	/// <summary>
	/// gate对外连接使用
	/// </summary>
	public class GateNetworkComponent: Component<World>, IUpdate, IStart
	{
		private IService service;

		private readonly Dictionary<ObjectId, AChannel> unitIdChannels =
				new Dictionary<ObjectId, AChannel>();

		private void Accept(string host, int port, NetworkProtocol protocol = NetworkProtocol.TCP)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.service = new TService(host, port);
					break;
				case NetworkProtocol.UDP:
					this.service = new UService(host, port);
					break;
				default:
					throw new ArgumentOutOfRangeException("protocol");
			}

			this.AcceptChannel();
		}

		public void Start()
		{
			this.Accept(World.Instance.Options.GateHost, World.Instance.Options.GatePort,
					World.Instance.Options.Protocol);
		}

		public void Update()
		{
			this.service.Update();
		}

		/// <summary>
		/// 接收连接
		/// </summary>
		private async void AcceptChannel()
		{
			while (true)
			{
				AChannel channel = await this.service.GetChannel();
				channel.OnDispose += this.OnChannelDispose;
				ProcessChannel(channel);
			}
		}

		/// <summary>
		/// 接收分发封包
		/// </summary>
		/// <param name="channel"></param>
		private static async void ProcessChannel(AChannel channel)
		{
			while (true)
			{
				byte[] messageBytes = await channel.RecvAsync();
				Env env = new Env();
				env[EnvKey.Channel] = channel;
				env[EnvKey.MessageBytes] = messageBytes;
				Opcode opcode = (Opcode)BitConverter.ToUInt16(messageBytes, 0);
				env[EnvKey.Opcode] = opcode;
				if (!MessageTypeHelper.IsClientMessage(opcode))
				{
					continue;
				}

#pragma warning disable 4014
				World.Instance.GetComponent<EventComponent<EventAttribute>>()
						.RunAsync(EventType.GateRecvClientMessage, env);
#pragma warning restore 4014
			}
		}

		// channel删除的时候需要清除与unit id的关联
		private void OnChannelDispose(AChannel channel)
		{
			ChannelUnitInfoComponent channelUnitInfoComponent =
					channel.GetComponent<ChannelUnitInfoComponent>();
			if (channelUnitInfoComponent != null)
			{
				this.unitIdChannels.Remove(channelUnitInfoComponent.UnitId);
			}
		}

		// 将unit id与channel关联起来
		public void AssociateUnitIdAndChannel(ObjectId id, AChannel channel)
		{
			this.unitIdChannels[id] = channel;
		}

		public void SendAsync(ObjectId id, byte[] buffer)
		{
			AChannel channel;
			if (!this.unitIdChannels.TryGetValue(id, out channel))
			{
				return;
			}

			channel.SendAsync(buffer);
		}
	}
}