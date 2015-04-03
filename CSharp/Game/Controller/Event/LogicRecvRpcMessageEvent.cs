using System;
using System.Threading.Tasks;
using Common.Network;
using Model;

namespace Controller
{
	[Event(EventType.LogicRecvRpcMessage, ServerType.All)]
	public class LogicRecvRpcMessageEventEvent : IEventAsync
	{
		public async Task RunAsync(Env env)
		{
			byte[] messageBytes = env.Get<byte[]>(EnvKey.MessageBytes);
			AChannel channel = env.Get<AChannel>(EnvKey.Channel);
			ushort opcode = env.Get<ushort>(EnvKey.Opcode);

			MessageParseHelper.LogicParseRpcRequestMessage(messageBytes, env);
			
			try
			{
				await World.Instance.GetComponent<MessageComponent>().RunAsync(opcode, env);
			}
			catch (Exception e)
			{
				int requestId = env.Get<int>(EnvKey.RpcRequestId);
				World.Instance.GetComponent<NetworkComponent>().RpcException(channel, requestId, 0, e.Message);
				throw;
			}
		}
	}
}