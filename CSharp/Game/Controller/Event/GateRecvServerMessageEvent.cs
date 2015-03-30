using System;
using Model;
using MongoDB.Bson;

namespace Controller
{
	[Event(EventType.GateRecvServerMessage, ServerType.Gate)]
	public class GateRecvServerMessageEvent : IEventSync
	{
		public void Run(Env env)
		{
			byte[] message = env.Get<byte[]>(EnvKey.Message);
			byte[] idBuffer = new byte[12];
			Array.Copy(message, 2, idBuffer, 0, 12);
			ObjectId unitId = new ObjectId(idBuffer);

			byte[] buffer = new byte[message.Length - 12];
			Array.Copy(message, 0, buffer, 0, 2);
			Array.Copy(message, 14, buffer, 2, message.Length - 14);
			World.Instance.GetComponent<GateNetworkComponent>().SendAsync(unitId, buffer);
		}
	}
}
