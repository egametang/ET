using System;

namespace ETModel
{
	public class SessionCallbackComponent: Component
	{
		public Action<Session, Packet> MessageCallback;
		public Action<Session> DisposeCallback;
	}
}
