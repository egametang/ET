using System.Threading.Tasks;
using BossBase;
using DataCenter;

namespace BossCommand
{
	public class BCServerInfo: ABossCommand
	{
		public BCServerInfo(IMessageChannel iMessageChannel, DataCenterEntities entities): 
			base(iMessageChannel, entities)
		{
		}

		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm { Message = "servers"});
			var smsgBossServersInfo = await this.RecvMessage<SMSG_Boss_ServersInfo>();
			return smsgBossServersInfo;
		}
	}
}
