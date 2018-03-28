using System.Net;
using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class NetOuterComponentAwakeSystem : AwakeSystem<NetOuterComponent>
	{
		public override void Awake(NetOuterComponent self)
		{
			self.Awake();
		}
	}

	[ObjectSystem]
	public class NetOuterComponentAwake1System : AwakeSystem<NetOuterComponent, IPEndPoint>
	{
		public override void Awake(NetOuterComponent self, IPEndPoint a)
		{
			self.Awake(a);
		}
	}

	[ObjectSystem]
	public class NetOuterComponentUpdateSystem : UpdateSystem<NetOuterComponent>
	{
		public override void Update(NetOuterComponent self)
		{
			self.Update();
		}
	}

	public static class NetOuterComponentEx
	{
		public static void Awake(this NetOuterComponent self)
		{
			self.Awake(NetworkProtocol.TCP);
			self.MessagePacker = new ProtobufPacker();
			self.MessageDispatcher = new OuterMessageDispatcher();
		}

		public static void Awake(this NetOuterComponent self, IPEndPoint ipEndPoint)
		{
			self.Awake(NetworkProtocol.TCP, ipEndPoint);
			self.MessagePacker = new ProtobufPacker();
			self.MessageDispatcher = new OuterMessageDispatcher();
		}

		public static void Update(this NetOuterComponent self)
		{
			self.Update();
		}
	}
}