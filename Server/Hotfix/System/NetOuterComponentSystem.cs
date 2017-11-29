using System.Net;
using Model;

namespace Hotfix
{
	[ObjectEvent]
	public class NetOuterComponentEvent : ObjectEvent<NetOuterComponent>, IAwake, IAwake<IPEndPoint>, IUpdate
	{
		public void Awake()
		{
			this.Get().Awake();
		}

		public void Awake(IPEndPoint ipEndPoint)
		{
			this.Get().Awake(ipEndPoint);
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

		public static void Awake(this NetOuterComponent self, IPEndPoint ipEndPoint)
		{
			self.Awake(NetworkProtocol.TCP, ipEndPoint);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new OuterMessageDispatcher();
		}

		public static void Update(this NetOuterComponent self)
		{
			self.Update();
		}
	}
}