

namespace ET
{
	public class NetInnerComponentAwakeSystem : AwakeSystem<NetInnerComponent>
	{
		public override void Awake(NetInnerComponent self)
		{
			NetInnerComponent.Instance = self;
			self.Awake(NetworkProtocol.TCP);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}
	}

	public class NetInnerComponentAwake1System : AwakeSystem<NetInnerComponent, string>
	{
		public override void Awake(NetInnerComponent self, string a)
		{
			NetInnerComponent.Instance = self;
			self.Awake(NetworkProtocol.TCP, a);
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}
	}
	
	public class NetInnerComponentLoadSystem : LoadSystem<NetInnerComponent>
	{
		public override void Load(NetInnerComponent self)
		{
			self.MessagePacker = new MongoPacker();
			self.MessageDispatcher = new InnerMessageDispatcher();
		}
	}

	public class NetInnerComponentUpdateSystem : UpdateSystem<NetInnerComponent>
	{
		public override void Update(NetInnerComponent self)
		{
			self.Update();
		}
	}
}