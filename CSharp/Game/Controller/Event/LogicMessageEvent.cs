using System;
using Model;
using MongoDB.Bson;

namespace Controller
{
	[Event(EventType.Message, ServerType.All)]
	public class LogicMessageEvent : IEventSync
	{
		public void Run(Env env)
		{
			byte[] message = env.Get<byte[]>(EnvKey.Message);

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

			World.Instance.GetComponent<EventComponent<MessageAttribute>>().RunAsync(opcode, env);
		}
	}
}
