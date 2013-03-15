using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
	public class BCAllowBuy: ABossCommand
	{
		public BCAllowBuy(IMessageChannel iMessageChannel): base(iMessageChannel)
		{
		}

		public string Guid { get; set; }

		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm
			{
				Message = string.Format("forbidden_buy_item {0} 0", this.Guid)
			});
			var smsgBossCommandResponse = await RecvMessage<SMSG_Boss_Command_Response>();
			if (smsgBossCommandResponse.ErrorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				return ErrorCode.RESPONSE_SUCCESS;
			}
			return smsgBossCommandResponse.ErrorCode;
		}
	}
}
