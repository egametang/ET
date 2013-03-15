using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
	public class BCForbiddenBuy: ABossCommand
	{
		public BCForbiddenBuy(IMessageChannel iMessageChannel): base(iMessageChannel)
		{
		}

		public string Guid { get; set; }
		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm
			{
				Message = string.Format("forbidden_buy_item {0} {1}", this.Guid, 365 * 24 * 3600)
			});

			var smsgBossCommandResponse = await this.RecvMessage<SMSG_Boss_Command_Response>();
			if (smsgBossCommandResponse.ErrorCode == ErrorCode.RESPONSE_SUCCESS)
			{
				return ErrorCode.RESPONSE_SUCCESS;
			}
			return smsgBossCommandResponse.ErrorCode;
		}
	}
}
