using ETModel;

namespace ETHotfix
{
	[ObjectSystem]
	public class NetInnerComponentAwakeSystem : AwakeSystem<NetInnerComponent>
	{
		public override void Awake(NetInnerComponent self)
		{
			NetInnerComponent.Instance = self;
			self.Awake(NetworkProtocol.TCP, Packet.PacketSizeLength4);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}
	}

	[ObjectSystem]
	public class NetInnerComponentAwake1System : AwakeSystem<NetInnerComponent, string>
	{
		public override void Awake(NetInnerComponent self, string a)
		{
			NetInnerComponent.Instance = self;
			self.Awake(NetworkProtocol.TCP, a, Packet.PacketSizeLength4);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}
	}
	
	[ObjectSystem]
	public class NetInnerComponentLoadSystem : LoadSystem<NetInnerComponent>
	{
		public override void Load(NetInnerComponent self)
		{
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
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
}