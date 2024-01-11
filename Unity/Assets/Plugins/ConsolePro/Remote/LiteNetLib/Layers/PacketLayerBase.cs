using System.Net;

namespace FlyingWormConsole3.LiteNetLib.Layers
{
    public abstract class PacketLayerBase
    {
        public readonly int ExtraPacketSizeForLayer;

        protected PacketLayerBase(int extraPacketSizeForLayer)
        {
            ExtraPacketSizeForLayer = extraPacketSizeForLayer;
        }

        public abstract void ProcessInboundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length);
        public abstract void ProcessOutBoundPacket(IPEndPoint endPoint, ref byte[] data, ref int offset, ref int length);
    }
}
