using System;
using Common.Base;
using Common.Event;
using Network;
using TNet;
using UNet;

namespace Model
{
	public class ServiceComponent: Component<World>
	{
		private IService service;

		public void Run(string host, int port, NetworkProtocol protocol = NetworkProtocol.TCP)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					service = new TService("127.0.0.1", 8888);
					break;
				case NetworkProtocol.UDP:
					service = new UService("127.0.0.1", 8888);
					break;
				default:
					throw new ArgumentOutOfRangeException("protocol");
			}
			
			service.Add(AcceptChannel);

			service.Run();
		}

		/// <summary>
		/// 接收连接
		/// </summary>
		private async void AcceptChannel()
		{
			while (true)
			{
				IChannel channel = await service.GetChannel();
				ProcessChannel(channel);
			}
		}

		/// <summary>
		/// 接收分发封包
		/// </summary>
		/// <param name="channel"></param>
		private static async void ProcessChannel(IChannel channel)
		{
			while (true)
			{
				byte[] message = await channel.RecvAsync();
				Env env = new Env();
				env[EnvKey.Message] = message;
				int opcode = BitConverter.ToUInt16(message, 0);
				World.Instance.GetComponent<EventComponent<MessageAttribute>>().Run(opcode, env);
			}
		}
	}
}