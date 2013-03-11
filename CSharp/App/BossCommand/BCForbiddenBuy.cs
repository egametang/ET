using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BossBase;
using DataCenter;
using Helper;

namespace BossCommand
{
	public class BCForbiddenBuy: ABossCommand
	{
		public BCForbiddenBuy(IMessageChannel iMessageChannel, DataCenterEntities entities): 
			base(iMessageChannel, entities)
		{
		}

		public string Guid { get; set; }
		public override async Task<object> DoAsync()
		{
			this.SendMessage(new CMSG_Boss_Gm { Message = string.Format("forbidden_buy_item {0} {1}", this.Guid, 365 * 24 * 3600) });

			var smsgBossCommandResponse = await this.RecvMessage<SMSG_Boss_Command_Response>();
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
				var newBuff = new t_city_buff
				{
					buff_guid = RandomHelper.RandUInt64(),
					buff_id = BuffId.BUFF_FORBIDDEN_PLAYER_BUY_ITEM,
					buff_time = 0,
					buff_values = "{}".ToByteArray(),
					character_guid = decimal.Parse(this.Guid),
					create_time = DateTime.Now,
					modify_time = DateTime.Now,
					stack = 1
				};
				Entities.t_city_buff.Add(newBuff);
				Entities.SaveChanges();
				return ErrorCode.RESPONSE_SUCCESS;
			}
			return smsgBossCommandResponse.ErrorCode;
		}
	}
}
