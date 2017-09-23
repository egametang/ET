using Model;

namespace Hotfix
{
	[ObjectEvent]
	public class NetInnerComponentEvent : ObjectEvent<NetInnerComponent>, IAwake, IAwake<string, int>, IUpdate
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
	
	public static class NetInnerComponentSystem
	{
		public static void Awake(this NetInnerComponent self)
		{
			self.Awake(NetworkProtocol.TCP);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}

		public static void Awake(this NetInnerComponent self, string host, int port)
		{
			self.Awake(NetworkProtocol.TCP, host, port);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}

		public static void Update(this NetInnerComponent self)
		{
			self.Update();
		}
	}
}