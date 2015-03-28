using System;
using Common.Base;
using Common.Network;
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
				byte[] message = await channel.RecvAsync();
				Env env = new Env();
				env[EnvKey.Channel] = channel;
				env[EnvKey.Message] = message;
				// 进行消息解析分发
#pragma warning disable 4014
				World.Instance.GetComponent<EventComponent<EventAttribute>>()
						.RunAsync(EventType.GateMessage, env);
#pragma warning restore 4014
			}
		}
	}
}