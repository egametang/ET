using System.Net;
using Model;

namespace Hotfix
{
	[ObjectSystem]
	public class NetInnerComponentSystem : ObjectSystem<NetInnerComponent>, IAwake, IAwake<IPEndPoint>, IUpdate
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
	
	public static class NetInnerComponentEx
	{
		public static void Awake(this NetInnerComponent self)
		{
			self.Awake(NetworkProtocol.TCP);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
			self.AppType = self.Entity.GetComponent<StartConfigComponent>().StartConfig.AppType;
		}

		public static void Awake(this NetInnerComponent self, IPEndPoint ipEndPoint)
		{
			self.Awake(NetworkProtocol.TCP, ipEndPoint);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
			self.AppType = self.Entity.GetComponent<StartConfigComponent>().StartConfig.AppType;
		}

		public static void Update(this NetInnerComponent self)
		{
			self.Update();
		}
	}
}