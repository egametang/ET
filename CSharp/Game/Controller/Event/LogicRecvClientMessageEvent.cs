using Model;
using MongoDB.Bson;

namespace Controller
{
	[Event(EventType.LogicRecvClientMessage, ServerType.All)]
	public class LogicRecvClientMessageEvent: IEventSync
	{
		public void Run(Env env)
		{
			byte[] messageBytes = env.Get<byte[]>(EnvKey.MessageBytes);
			// 如果是客户端消息,转交给unit actor处理
			MessageParseHelper.LogicParseClientToGateToLogicMessage(messageBytes, env);
			ObjectId unitId = env.Get<ObjectId>(EnvKey.MessageUnitId);
			Actor actor = World.Instance.GetComponent<ActorComponent>().Get(unitId);
			if (actor != null)
			{
				actor.Add(env);
			}
		}
	}
}