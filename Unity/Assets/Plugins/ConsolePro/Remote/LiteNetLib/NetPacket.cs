using System;
using System.Net;
using FlyingWormConsole3.LiteNetLib.Utils;

namespace FlyingWormConsole3.LiteNetLib
{
    internal enum PacketProperty : byte
    {
        Unreliable,
        Channeled,
        Ack,
        Ping,
        Pong,
        ConnectRequest,
        ConnectAccept,
        Disconnect,
        UnconnectedMessage,
        MtuCheck,
        MtuOk,
        Broadcast,
        Merged,
        ShutdownOk,
        PeerNotFound,
        InvalidProtocol,
        NatMessage,
        Empty
    }

    internal sealed class NetPacket
    {
        private static readonly int LastProperty = Enum.GetValues(typeof(PacketProperty)).Length;
        private static readonly int[] HeaderSizes;

        static NetPacket()
        {
            HeaderSizes = new int[LastProperty+1];
            for (int i = 0; i < HeaderSizes.Length; i++)
            {
                switch ((PacketProperty)i)
                {
                    case PacketProperty.Channeled:
                    case PacketProperty.Ack:
                        HeaderSizes[i] = NetConstants.ChanneledHeaderSize;
                        break;
                    case PacketProperty.Ping:
                        HeaderSizes[i] = NetConstants.HeaderSize + 2;
                        break;
                    case PacketProperty.ConnectRequest:
                        HeaderSizes[i] = NetConnectRequestPacket.HeaderSize;
                        break;
                    case PacketProperty.ConnectAccept:
                        HeaderSizes[i] = NetConnectAcceptPacket.Size;
                        break;
                    case PacketProperty.Disconnect:
                        HeaderSizes[i] = NetConstants.HeaderSize + 8;
                        break;
                    case PacketProperty.Pong:
                        HeaderSizes[i] = NetConstants.HeaderSize + 10;
                        break;
                    default:
                        HeaderSizes[i] = NetConstants.HeaderSize;
                        break;
                }
            }
        }

        //Header
        public PacketProperty Property
        {
            get { return (PacketProperty)(RawData[0] & 0x1F); }
            set { RawData[0] = (byte)((RawData[0] & 0xE0) | (byte)value); }
        }

        public byte ConnectionNumber
        {
            get { return (byte)((RawData[0] & 0x60) >> 5); }
            set { RawData[0] = (byte) ((RawData[0] & 0x9F) | (value << 5)); }
        }

        public ushort Sequence
        {
            get { return BitConverter.ToUInt16(RawData, 1); }
            set { FastBitConverter.GetBytes(RawData, 1, value); }
        }

        public bool IsFragmented
        {
            get { return (RawData[0] & 0x80) != 0; }
        }

        public void MarkFragmented()
        {
            RawData[0] |= 0x80; //set first bit
        }

        public byte ChannelId
        {
            get { return RawData[3]; }
            set { RawData[3] = value; }
        }

        public ushort FragmentId
        {
            get { return BitConverter.ToUInt16(RawData, 4); }
            set { FastBitConverter.GetBytes(RawData, 4, value); }
        }

        public ushort FragmentPart
        {
            get { return BitConverter.ToUInt16(RawData, 6); }
            set { FastBitConverter.GetBytes(RawData, 6, value); }
        }

        public ushort FragmentsTotal
        {
            get { return BitConverter.ToUInt16(RawData, 8); }
            set { FastBitConverter.GetBytes(RawData, 8, value); }
        }

        //Data
        public byte[] RawData;
        public int Size;

        //Delivery
        public object UserData;

        //Pool node
        public NetPacket Next;

#if DEBUG_REFCOUNT
        public int RefCount = 1;
#endif

        public NetPacket(int size)
        {
            RawData = new byte[size];
            Size = size;
        }

        public NetPacket(PacketProperty property, int size)
        {
            size += GetHeaderSize(property);
            RawData = new byte[size];
            Property = property;
            Size = size;
        }

        public static int GetHeaderSize(PacketProperty property)
        {
            return HeaderSizes[(int)property];
        }

        public int GetHeaderSize()
        {
            return HeaderSizes[RawData[0] & 0x1F];
        }

        public bool Verify()
        {
            byte property = (byte)(RawData[0] & 0x1F);
            if (property > LastProperty)
                return false;
            int headerSize = HeaderSizes[property];
            bool fragmented = (RawData[0] & 0x80) != 0;
            return Size >= headerSize && (!fragmented || Size >= headerSize + NetConstants.FragmentHeaderSize);
        }
    }

    internal sealed class NetConnectRequestPacket
    {
        public const int HeaderSize = 14;
        public readonly long ConnectionTime;
        public readonly byte ConnectionNumber;
        public readonly byte[] TargetAddress;
        public readonly NetDataReader Data;

        private NetConnectRequestPacket(long connectionTime, byte connectionNumber, byte[] targetAddress, NetDataReader data)
        {
            ConnectionTime = connectionTime;
            ConnectionNumber = connectionNumber;
            TargetAddress = targetAddress;
            Data = data;
        }

        public static int GetProtocolId(NetPacket packet)
        {
            return BitConverter.ToInt32(packet.RawData, 1);
        }
        
        public static NetConnectRequestPacket FromData(NetPacket packet)
        {
            if (packet.ConnectionNumber >= NetConstants.MaxConnectionNumber)
                return null;

            //Getting new id for peer
            long connectionId = BitConverter.ToInt64(packet.RawData, 5);
            
            //Get target address
            int addrSize = packet.RawData[13];
            if (addrSize != 16 && addrSize != 28)
                return null;
            byte[] addressBytes = new byte[addrSize];
            Buffer.BlockCopy(packet.RawData, 14, addressBytes, 0, addrSize);

            // Read data and create request
            var reader = new NetDataReader(null, 0, 0);
            if (packet.Size > HeaderSize+addrSize)
                reader.SetSource(packet.RawData, HeaderSize + addrSize, packet.Size);

            return new NetConnectRequestPacket(connectionId, packet.ConnectionNumber, addressBytes, reader);
        }

        public static NetPacket Make(NetDataWriter connectData, SocketAddress addressBytes, long connectId)
        {
            //Make initial packet
            var packet = new NetPacket(PacketProperty.ConnectRequest, connectData.Length+addressBytes.Size);

            //Add data
            FastBitConverter.GetBytes(packet.RawData, 1, NetConstants.ProtocolId);
            FastBitConverter.GetBytes(packet.RawData, 5, connectId);
            packet.RawData[13] = (byte)addressBytes.Size;
            for (int i = 0; i < addressBytes.Size; i++)
                packet.RawData[14+i] = addressBytes[i];
            Buffer.BlockCopy(connectData.Data, 0, packet.RawData, 14+addressBytes.Size, connectData.Length);
            return packet;
        }
    }

    internal sealed class NetConnectAcceptPacket
    {
        public const int Size = 11;
        public readonly long ConnectionId;
        public readonly byte ConnectionNumber;
        public readonly bool IsReusedPeer;

        private NetConnectAcceptPacket(long connectionId, byte connectionNumber, bool isReusedPeer)
        {
            ConnectionId = connectionId;
            ConnectionNumber = connectionNumber;
            IsReusedPeer = isReusedPeer;
        }

        public static NetConnectAcceptPacket FromData(NetPacket packet)
        {
            if (packet.Size > Size)
                return null;

            long connectionId = BitConverter.ToInt64(packet.RawData, 1);
            //check connect num
            byte connectionNumber = packet.RawData[9];
            if (connectionNumber >= NetConstants.MaxConnectionNumber)
                return null;
            //check reused flag
            byte isReused = packet.RawData[10];
            if (isReused > 1)
                return null;

            return new NetConnectAcceptPacket(connectionId, connectionNumber, isReused == 1);
        }

        public static NetPacket Make(long connectId, byte connectNum, bool reusedPeer)
        {
            var packet = new NetPacket(PacketProperty.ConnectAccept, 0);
            FastBitConverter.GetBytes(packet.RawData, 1, connectId);
            packet.RawData[9] = connectNum;
            packet.RawData[10] = (byte)(reusedPeer ? 1 : 0);
            return packet;
        }
    }
}
