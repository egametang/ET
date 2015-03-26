using Common.Helper;
using Model;
using Common.Network;

namespace Controller.Message
{
	public class CMsgLogin
	{
		public byte[] Account { get; set; }
		public byte[] PassMd5 { get; set; }
	}

	[Message(MessageType.CMsgLogin)]
	internal class CMsgLoginEvent: IEventSync
	{
		public void Run(Env env)
		{
			var messageBytes = env.Get<byte[]>(EnvKey.Message);
			CMsgLogin cmsg = MongoHelper.FromBson<CMsgLogin>(messageBytes, 2);
			Unit unit = World.Instance.GetComponent<FactoryComponent<Unit>>().Create(UnitType.GatePlayer, 1);

			AChannel channel = env.Get<AChannel>(EnvKey.Channel);
			ChannelUnitInfoComponent channelUnitInfoComponent = channel.AddComponent<ChannelUnitInfoComponent>();
			channelUnitInfoComponent.Account = cmsg.Account;
			channelUnitInfoComponent.PlayerId = unit.Id;
		}
	}
}
