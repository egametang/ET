using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginClient
{
	public interface IMessageChannel : IDisposable
	{
		void SendMessage<T>(ushort opcode, T message, byte channelID = 0);
		Task<Tuple<ushort, byte[]>> RecvMessage();
	}
}
