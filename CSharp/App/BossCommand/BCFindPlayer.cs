using System.Threading.Tasks;
using BossBase;

namespace BossCommand
{
	public class BCFindPlayer: ABossCommand
	{
		public BCFindPlayer(IMessageChannel iMessageChannel): base(iMessageChannel)
		{
		}

		public int FindTypeIndex { get; set; }

		public string FindType { get; set; }

		public override Task<object> DoAsync()
		{
			return null;
		}
	}
}
