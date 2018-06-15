using System.Net;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class NetInnerComponentAwakeSystem : AwakeSystem<NetInnerComponent>
	{
		public override void Awake(NetInnerComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class NetInnerComponentAwake1System : AwakeSystem<NetInnerComponent, IPEndPoint>
	{
		public override void Awake(NetInnerComponent self, IPEndPoint a)
		{
			self.Awake(a);
		}
	}

	[ObjectSystem]
	public class NetInnerComponentUpdateSystem : UpdateSystem<NetInnerComponent>
	{
		public override void Update(NetInnerComponent self)
		{
			self.Update();
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