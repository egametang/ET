using System.Linq;
using System.Threading.Tasks;
using BossBase;
using DataCenter;

namespace BossCommand
{
	public class BCAllowBuy: ABossCommand
	{
		public BCAllowBuy(IMessageChannel iMessageChannel, DataCenterEntities entities): 
			base(iMessageChannel, entities)
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
			if (smsgBossCommandResponse.ErrorCode == ErrorCode.BOSS_PLAYER_NOT_FOUND)
			{
				decimal character_guid = decimal.Parse(this.Guid);
				var removeBuffs = Entities.t_city_buff.Where(
					c => c.buff_id == BuffId.BUFF_FORBIDDEN_PLAYER_BUY_ITEM &&
						c.character_guid == character_guid);
				foreach (var removeBuff in removeBuffs)
				{
					Entities.t_city_buff.Remove(removeBuff);
				}
				Entities.SaveChanges();

				return ErrorCode.RESPONSE_SUCCESS;
			}
			return smsgBossCommandResponse.ErrorCode;
		}
	}
}
