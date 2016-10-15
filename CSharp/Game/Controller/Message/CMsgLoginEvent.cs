using System.Threading.Tasks;
using Model;

namespace Controller
{
	public class CMsgLogin
	{
		public byte[] Account { get; set; }
		public byte[] PassMd5 { get; set; }
	}

	[Message(ServerType.Gate)]
	internal class CMsgLoginEvent : MEvent<CMsgLogin, Task<bool>>
	{
		public override async Task<bool> Run(CMsgLogin msg)
		{
			return true;
		}
	}
}