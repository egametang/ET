using System;
using System.Threading.Tasks;

namespace BossBase
{
    public interface IMessageChannel: IDisposable
    {
        void SendMessage<T>(ushort opcode, T message, byte channelID = 0);
        Task<Tuple<ushort, byte[]>> RecvMessage();
    }
}