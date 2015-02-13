using System;
using Common.Base;
using Common.Event;
using Common.Network;
using TNet;
using UNet;

namespace Model
{
	public class NetworkComponent: Component<World>
	{
		private IService service;

		public void Run(string host, int port, NetworkProtocol protocol = NetworkProtocol.TCP)
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

			this.service.Add(this.AcceptChannel);

			this.service.Start();
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
#pragma warning disable 4014
				World.Instance.GetComponent<EventComponent<ActionAttribute>>()
						.RunAsync(ActionType.MessageAction, env);
#pragma warning restore 4014
			}
		}
	}
}