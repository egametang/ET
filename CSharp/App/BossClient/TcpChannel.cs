using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using BossBase;
using Helper;
using Logger;

namespace BossClient
{
    public class TcpChannel: IMessageChannel
    {
        private readonly NetworkStream networkStream;

        public TcpChannel(TcpClient tcpClient)
        {
            this.networkStream = tcpClient.GetStream();
        }

        public void Dispose()
        {
            this.networkStream.Dispose();
        }

        public async void SendMessage<T>(ushort opcode, T message, byte channelID = 0)
        {
            byte[] protoBytes = ProtobufHelper.ToBytes(message);
            var neworkBytes = new byte[sizeof (int) + sizeof (ushort) + protoBytes.Length];

            int totalSize = sizeof (ushort) + protoBytes.Length;

            var totalSizeBytes = BitConverter.GetBytes(totalSize);
            totalSizeBytes.CopyTo(neworkBytes, 0);

            var opcodeBytes = BitConverter.GetBytes(opcode);
            opcodeBytes.CopyTo(neworkBytes, sizeof (int));

            protoBytes.CopyTo(neworkBytes, sizeof (int) + sizeof (ushort));

            await this.networkStream.WriteAsync(neworkBytes, 0, neworkBytes.Length);
        }

        public async Task<Tuple<ushort, byte[]>> RecvMessage()
        {
            int totalReadSize = 0;
            int needReadSize = sizeof (int);
            var packetBytes = new byte[needReadSize];
            while (totalReadSize != needReadSize)
            {
                int readSize =
                        await
                                this.networkStream.ReadAsync(packetBytes, totalReadSize,
                                        packetBytes.Length);
                if (readSize == 0)
                {
                    throw new BossException("connection closed");
                }
                totalReadSize += readSize;
            }

            int packetSize = BitConverter.ToInt32(packetBytes, 0);
            Log.Debug("packetSize: {0}", packetSize);

            // 读opcode和message
            totalReadSize = 0;
            needReadSize = packetSize;
            var contentBytes = new byte[needReadSize];
            while (totalReadSize != needReadSize)
            {
                int readSize =
                        await
                                this.networkStream.ReadAsync(contentBytes, totalReadSize,
                                        contentBytes.Length);
                if (readSize == 0)
                {
                    throw new BossException("connection closed");
                }
                totalReadSize += readSize;
            }

            ushort opcode = BitConverter.ToUInt16(contentBytes, 0);

            var messageBytes = new byte[needReadSize - sizeof (ushort)];
            Array.Copy(contentBytes, sizeof (ushort), messageBytes, 0, messageBytes.Length);

            return Tuple.Create(opcode, messageBytes);
        }
    }
}