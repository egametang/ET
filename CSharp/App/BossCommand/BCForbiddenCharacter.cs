using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
	public class BCForbiddenCharacter: ABossCommand
	{
		public BCForbiddenCharacter(IMessageChannel iMessageChannel): base(iMessageChannel)
		{
		}

		public string Guid { get; set; }
		public string Command { get; set; }
		public string ForbiddenTime { get; set; }
		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm
			{
				Message = string.Format("{0} {1} {2}", this.Command, this.Guid, this.ForbiddenTime)
			});

			var smsgBossCommandResponse = await this.RecvMessage<SMSG_Boss_Command_Response>();
			return smsgBossCommandResponse.ErrorCode;
		}
	}
}
