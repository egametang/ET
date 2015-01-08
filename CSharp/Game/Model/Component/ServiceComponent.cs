using System;
using Common.Base;
using Common.Event;
using Network;
using TNet;

namespace Model
{
	public class ServiceComponent: Component<World>
	{
		private IService service;

		public void Run(string host, int port)
		{
			service = new TService("127.0.0.1", 8888);
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
				byte[] packet = await channel.RecvAsync();
				Env env = new Env();
				env[EnvKey.Packet] = packet;
				int opcode = BitConverter.ToUInt16(packet, 0);
				World.Instance.GetComponent<EventComponent<MessageAttribute>>().Run(opcode, env);
			}
		}
	}
}