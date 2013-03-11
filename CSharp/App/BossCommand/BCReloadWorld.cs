using BossBase;
using DataCenter;

namespace BossCommand
{
	public class BCReloadWorld: ABossCommand
	{
		public BCReloadWorld(IMessageChannel iMessageChannel, DataCenterEntities entities): 
			base(iMessageChannel, entities)
		{
		}

		public override object Do()
		{
			this.SendMessage(new CMSG_Boss_Gm { Message = "reload" });
			return null;
		}
	}
}
