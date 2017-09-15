#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
﻿using System;
using System.Collections.Generic;
using FlyingWormConsole3.LiteNetLib.Utils;

namespace FlyingWormConsole3.LiteNetLib
{
    internal class NetPacketPool
    {
        private readonly Stack<NetPacket> _pool;

        public NetPacketPool()
        {
            _pool = new Stack<NetPacket>();
        }

        public NetPacket GetWithData(PacketProperty property, NetDataWriter writer)
        {
            var packet = Get(property, writer.Length);
            Buffer.BlockCopy(writer.Data, 0, packet.RawData, NetPacket.GetHeaderSize(property), writer.Length);
            return packet;
        }

        public NetPacket GetWithData(PacketProperty property, byte[] data, int start, int length)
        {
            var packet = Get(property, length);
            Buffer.BlockCopy(data, start, packet.RawData, NetPacket.GetHeaderSize(property), length);
            return packet;
        }

        //Get packet just for read
        public NetPacket GetAndRead(byte[] data, int start, int count)
        {
            NetPacket packet = null;
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    packet = _pool.Pop();
                }
            }
            if (packet == null)
            {
                //allocate new packet of max size or bigger
                packet = new NetPacket(NetConstants.MaxPacketSize);
            }
            if (!packet.FromBytes(data, start, count))
            {
                Recycle(packet);
                return null;
            }
            return packet;
        }

        //Get packet with size
        public NetPacket Get(PacketProperty property, int size)
        {
            NetPacket packet = null;
            size += NetPacket.GetHeaderSize(property);
            if (size <= NetConstants.MaxPacketSize)
            {
                lock (_pool)
                {
                    if (_pool.Count > 0)
                    {
                        packet = _pool.Pop();
                    }
                }
            }
            if (packet == null)
            {
                //allocate new packet of max size or bigger
                packet = new NetPacket(size > NetConstants.MaxPacketSize ? size : NetConstants.MaxPacketSize);
            }
            else
            {
                Array.Clear(packet.RawData, 0, size);
            }
            packet.Property = property;
            packet.Size = size;
            return packet;
        }

        public void Recycle(NetPacket packet)
        { 
            if (packet.Size > NetConstants.MaxPacketSize)
            {
                //Dont pool big packets. Save memory
                return;
            }

            //Clean fragmented flag
            packet.IsFragmented = false;
            lock (_pool)
            {
                _pool.Push(packet);
            }
        }
    }
}
#endif
