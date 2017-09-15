#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
using System;
using FlyingWormConsole3.LiteNetLib.Utils;

namespace FlyingWormConsole3.LiteNetLib
{
    internal enum PacketProperty : byte
    {
        Unreliable,             //0
        Reliable,               //1
        Sequenced,              //2
        ReliableOrdered,        //3
        AckReliable,            //4
        AckReliableOrdered,     //5
        Ping,                   //6
        Pong,                   //7
        ConnectRequest,         //8
        ConnectAccept,          //9
        Disconnect,             //10
        UnconnectedMessage,     //11
        NatIntroductionRequest, //12
        NatIntroduction,        //13
        NatPunchMessage,        //14
        MtuCheck,               //15
        MtuOk,                  //16
        DiscoveryRequest,       //17
        DiscoveryResponse,      //18
        Merged                  //19
    }

    internal sealed class NetPacket
    {
        private const int LastProperty = 19;

        //Header
        public PacketProperty Property
        {
            get { return (PacketProperty)(RawData[0] & 0x7F); }
            set { RawData[0] = (byte)((RawData[0] & 0x80) | ((byte)value & 0x7F)); }
        }

        public ushort Sequence
        {
            get { return BitConverter.ToUInt16(RawData, 1); }
            set { FastBitConverter.GetBytes(RawData, 1, value); }
        }

        public bool IsFragmented
        {
            get { return (RawData[0] & 0x80) != 0; }
            set
            {
                if (value)
                    RawData[0] |= 0x80; //set first bit
                else
                    RawData[0] &= 0x7F; //unset first bit
            }
        }

        public ushort FragmentId
        {
            get { return BitConverter.ToUInt16(RawData, 3); }
            set { FastBitConverter.GetBytes(RawData, 3, value); }
        }

        public ushort FragmentPart
        {
            get { return BitConverter.ToUInt16(RawData, 5); }
            set { FastBitConverter.GetBytes(RawData, 5, value); }
        }

        public ushort FragmentsTotal
        {
            get { return BitConverter.ToUInt16(RawData, 7); }
            set { FastBitConverter.GetBytes(RawData, 7, value); }
        }

        //Data
        public readonly byte[] RawData;
        public int Size;

        public NetPacket(int size)
        {
            RawData = new byte[size];
            Size = 0;
        }

        public static bool GetPacketProperty(byte[] data, out PacketProperty property)
        {
            byte properyByte = (byte)(data[0] & 0x7F);
            if (properyByte > LastProperty)
            {
                property = PacketProperty.Unreliable;
                return false;
            }
            property = (PacketProperty)properyByte;
            return true;
        }

        public static int GetHeaderSize(PacketProperty property)
        {
            return IsSequenced(property)
                ? NetConstants.SequencedHeaderSize
                : NetConstants.HeaderSize;
        }

        public int GetHeaderSize()
        {
            return GetHeaderSize(Property);
        }

        public byte[] GetPacketData()
        {
            int headerSize = GetHeaderSize(Property);
            int dataSize = Size - headerSize;
            byte[] data = new byte[dataSize];
            Buffer.BlockCopy(RawData, headerSize, data, 0, dataSize);
            return data;
        }

        public bool IsClientData()
        {
            var property = Property;
            return property == PacketProperty.Reliable ||
                   property == PacketProperty.ReliableOrdered ||
                   property == PacketProperty.Unreliable ||
                   property == PacketProperty.Sequenced;
        }

        public static bool IsSequenced(PacketProperty property)
        {
            return property == PacketProperty.ReliableOrdered ||
                property == PacketProperty.Reliable ||
                property == PacketProperty.Sequenced ||
                property == PacketProperty.Ping ||
                property == PacketProperty.Pong ||
                property == PacketProperty.AckReliable ||
                property == PacketProperty.AckReliableOrdered;
        }

        //Packet contstructor from byte array
        public bool FromBytes(byte[] data, int start, int packetSize)
        {
            //Reading property
            byte property = (byte)(data[start] & 0x7F);
            bool fragmented = (data[start] & 0x80) != 0;
            int headerSize = GetHeaderSize((PacketProperty) property);

            if (property > LastProperty ||
                packetSize > NetConstants.PacketSizeLimit ||
                packetSize < headerSize ||
                (fragmented && packetSize < headerSize + NetConstants.FragmentHeaderSize))
            {
                return false;
            }

            Buffer.BlockCopy(data, start, RawData, 0, packetSize);
            Size = packetSize;
            return true;
        }
    }
}
#endif
