using System;
using Model;
using MongoDB.Bson;

namespace Controller
{
	[Event(EventType.GateRecvServerMessage, ServerType.Gate)]
	public class GateRecvServerMessageEvent: IEventSync
	{
		public void Run(Env env)
		{
			byte[] messageBytes = env.Get<byte[]>(EnvKey.MessageBytes);
			byte[] idBuffer = new byte[12];
			Array.Copy(messageBytes, 2, idBuffer, 0, 12);
			ObjectId unitId = new ObjectId(idBuffer);

			byte[] buffer = MessageParseHelper.LogicToGateMessageChangeToClientMessage(messageBytes);
			World.Instance.GetComponent<GateNetworkComponent>().SendAsync(unitId, buffer);
		}
	}
}