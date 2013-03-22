using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
	public class BCCommand : ABossCommand
	{
		public string Command { get; set; }

		public BCCommand(IMessageChannel iMessageChannel) : base(iMessageChannel)
		{
		}

		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm { Message = this.Command });

			var smsgBossCommandResponse = await this.RecvMessage<SMSG_Boss_Command_Response>();
			return smsgBossCommandResponse;
		}
	}
}
