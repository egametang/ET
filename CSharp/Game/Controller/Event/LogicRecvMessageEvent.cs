using System;
using System.Threading.Tasks;
using Common.Network;
using Model;
using MongoDB.Bson;

namespace Controller
{
	[Event(EventType.LogicRecvMessage, ServerType.All)]
	public class LogicRecvMessageEvent: IEventAsync
	{
		public async Task RunAsync(Env env)
		{
			byte[] message = env.Get<byte[]>(EnvKey.Message);
			AChannel channel = env.Get<AChannel>(EnvKey.Channel);
			int opcode = BitConverter.ToUInt16(message, 0);
			// 如果是客户端消息,转交给unit actor处理
			// 逻辑服收到客户端消息opcode(2) + id(12) + content
			if (MessageTypeHelper.IsClientMessage(opcode))
			{
				byte[] idBuffer = new byte[12];
				Array.Copy(message, 2, idBuffer, 0, 12);
				ObjectId unitId = new ObjectId(idBuffer);
				Unit unit = World.Instance.GetComponent<UnitComponent>().Get(unitId);
				if (unit != null)
				{
					unit.GetComponent<ActorComponent>().Add(env);
				}
				return;
			}
			
			try
			{
				await World.Instance.GetComponent<EventComponent<MessageAttribute>>().RunAsync(opcode, env);
			}
			catch (Exception e)
			{
				// 如果是rpc请求,统一处理一下异常
				if (MessageTypeHelper.IsRpcRequestMessage(opcode))
				{
					int id = BitConverter.ToInt32(message, 4);
					World.Instance.GetComponent<NetworkComponent>().ResponseException(channel, id, 0, e.Message);
				}
				throw;
			}
		}
	}
}