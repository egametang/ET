using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using BossBase;
using ENet;
using Helper;

namespace BossClient
{
    internal class ENetChannel: IMessageChannel
    {
        private readonly ESocket eSocket;

        public ENetChannel(ESocket eSocket)
        {
            this.eSocket = eSocket;
        }

        public async void Dispose()
        {
            await this.eSocket.DisconnectLaterAsync();
            this.eSocket.Dispose();
        }

        public void SendMessage<T>(ushort opcode, T message, byte channelID = 0)
        {
            byte[] protoBytes = ProtobufHelper.ToBytes(message);
            var neworkBytes = new byte[sizeof (ushort) + protoBytes.Length];

            var opcodeBytes = BitConverter.GetBytes(opcode);
            opcodeBytes.CopyTo(neworkBytes, 0);
            protoBytes.CopyTo(neworkBytes, sizeof (ushort));
            this.eSocket.WriteAsync(neworkBytes, channelID);
        }

        public async Task<Tuple<ushort, byte[]>> RecvMessage()
        {
            var bytes = await this.eSocket.ReadAsync();
            const int opcodeSize = sizeof (ushort);
            ushort opcode = BitConverter.ToUInt16(bytes, 0);
            byte flag = bytes[2];

            switch (flag)
            {
                case 0:
                {
                    var messageBytes = new byte[bytes.Length - opcodeSize - 1];
                    Array.Copy(bytes, opcodeSize + 1, messageBytes, 0, messageBytes.Length);
                    return Tuple.Create(opcode, messageBytes);
                }
                default:
                {
                    var decompressStream = new MemoryStream();
                    using (
                            var zipStream =
                                    new GZipStream(
                                            new MemoryStream(bytes, opcodeSize + 5,
                                                    bytes.Length - opcodeSize - 5),
                                            CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(decompressStream);
                    }
                    var decompressBytes = decompressStream.ToArray();
                    return Tuple.Create(opcode, decompressBytes);
                }
            }
        }
    }
}