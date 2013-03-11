using System;
using System.Linq;
using BossBase;
using DataCenter;

namespace BossCommand
{
	public class BCFindPlayer: ABossCommand
	{
		public BCFindPlayer(IMessageChannel iMessageChannel, DataCenterEntities entities): 
			base(iMessageChannel, entities)
		{
		}

		public int FindTypeIndex { get; set; }

		public string FindType { get; set; }

		public override object Do()
		{
			t_character result = null;
			switch (this.FindTypeIndex)
			{
				case 0:
				{
					result = Entities.t_character.FirstOrDefault(
						c => c.account == this.FindType);
					break;
				}
				case 1:
				{
					result = Entities.t_character.FirstOrDefault(
						c => c.character_name == this.FindType);
					break;
				}
				case 2:
				{
					var findGuid = Decimal.Parse(this.FindType);
					result = Entities.t_character.FirstOrDefault(
						c => c.character_guid == findGuid);
					break;
				}
			}
			return result;
		}
	}
}
