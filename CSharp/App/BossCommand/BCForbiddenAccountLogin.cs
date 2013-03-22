using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
	public class BCForbiddenAccountLogin: ABossCommand
	{
		public string Account { get; set; }
		public string ForbiddenLoginTime { get; set; }
		public BCForbiddenAccountLogin(IMessageChannel iMessageChannel): base(iMessageChannel)
		{
		}

		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm
			{
				Message = string.Format("forbid_account {0} {1}", this.Account, this.ForbiddenLoginTime)
			});
			var smsgBossCommandResponse = await RecvMessage<SMSG_Boss_Command_Response>();
			return smsgBossCommandResponse.ErrorCode;
		}
	}
}
