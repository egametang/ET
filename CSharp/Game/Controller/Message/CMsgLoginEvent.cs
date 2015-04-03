using Common.Network;
using Model;

namespace Controller
{
	public class CMsgLogin
	{
		public byte[] Account { get; set; }
		public byte[] PassMd5 { get; set; }
	}

	[Message(Opcode.CMsgLogin, typeof(CMsgLogin), ServerType.Gate)]
	internal class CMsgLoginEvent: IEventSync
	{
		public void Run(Env env)
		{
			CMsgLogin cmsg = env.Get<CMsgLogin>(EnvKey.Message);
			Unit unit = World.Instance.GetComponent<FactoryComponent<Unit>>().Create(UnitType.GatePlayer, 1);

			AChannel channel = env.Get<AChannel>(EnvKey.Channel);
			ChannelUnitInfoComponent channelUnitInfoComponent =
					channel.AddComponent<ChannelUnitInfoComponent>();
			channelUnitInfoComponent.Account = cmsg.Account;
			channelUnitInfoComponent.UnitId = unit.Id;
			World.Instance.GetComponent<GateNetworkComponent>().AssociateUnitIdAndChannel(unit.Id, channel);
		}
	}
}