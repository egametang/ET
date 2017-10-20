using Model;

namespace Hotfix
{
	[ObjectEvent]
	public class NetOuterComponentEvent : ObjectEvent<NetOuterComponent>, IAwake, IAwake<string, int>, IUpdate
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Awake(string a, int b)
		{
			this.Get().Awake(a, b);
		}

		public void Update()
		{
			this.Get().Update();
		}
	}

	public static class NetOuterComponentSystem
	{
		public static void Awake(this NetOuterComponent self)
		{
			self.Awake(NetworkProtocol.TCP);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new OuterMessageDispatcher();
		}

		public static void Awake(this NetOuterComponent self, string host, int port)
		{
			self.Awake(NetworkProtocol.TCP, host, port);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new OuterMessageDispatcher();
		}

		public static void Update(this NetOuterComponent self)
		{
			self.Update();
		}
	}
}