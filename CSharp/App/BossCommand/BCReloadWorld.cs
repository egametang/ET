using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
    public class BCReloadWorld: ABossCommand
    {
        public BCReloadWorld(IMessageChannel iMessageChannel): base(iMessageChannel)
        {
        }

        public override Task<object> DoAsync()
        {
            this.SendMessage(new CMSG_Boss_Gm { Message = "reload" });
            return null;
        }
    }
}