using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
	public class BCForbidLogin: ABossCommand
	{
		public string Command { get; set; }
		public string Content { get; set; }
		public string ForbiddenLoginTime { get; set; }
		public BCForbidLogin(IMessageChannel iMessageChannel): base(iMessageChannel)
		{
		}

		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm
			{
				Message = string.Format("{0} {1} {2}", this.Command, this.Content, this.ForbiddenLoginTime)
			});
			var smsgBossCommandResponse = await RecvMessage<SMSG_Boss_Command_Response>();
			return smsgBossCommandResponse.ErrorCode;
		}
	}
}
