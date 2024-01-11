using FlyingWormConsole3.LiteNetLib.Utils;
using System;
using System.Net;

namespace FlyingWormConsole3.LiteNetLib.Layers
{
    public sealed class Crc32cLayer : PacketLayerBase
    {
        public Crc32cLayer() : base(CRC32C.ChecksumSize)
        {

        }

        public override void ProcessInboundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
        {
            if (length < NetConstants.HeaderSize + CRC32C.ChecksumSize)
            {
                NetDebug.WriteError("[NM] DataReceived size: bad!");
                //Set length to 0 to have netManager drop the packet.
                length = 0;
                return;
            }

            int checksumPoint = length - CRC32C.ChecksumSize;
            if (CRC32C.Compute(data, offset, checksumPoint) != BitConverter.ToUInt32(data, checksumPoint))
            {
                NetDebug.Write("[NM] DataReceived checksum: bad!");
                //Set length to 0 to have netManager drop the packet.
                length = 0;
                return;
            }
            length -= CRC32C.ChecksumSize;
        }

        public override void ProcessOutBoundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length)
        {
            FastBitConverter.GetBytes(data, length, CRC32C.Compute(data, offset, length));
            length += CRC32C.ChecksumSize;
        }
    }
}
