#if DEBUG && !UNITY_WP_8_1 && !UNITY_WSA_8_1
using System;
using System.Collections.Generic;
using System.Threading;

namespace FlyingWormConsole3.LiteNetLib
{
    internal sealed class ReliableChannel
    {
        private class PendingPacket
        {
            public NetPacket Packet;
            public DateTime? TimeStamp;

            public NetPacket GetAndClear()
            {
                var packet = Packet;
                Packet = null;
                TimeStamp = null;
                return packet;
            }
        }

        private readonly Queue<NetPacket> _outgoingPackets;
        private readonly bool[] _outgoingAcks;               //for send acks
        private readonly PendingPacket[] _pendingPackets;    //for unacked packets and duplicates
        private readonly NetPacket[] _receivedPackets;       //for order
        private readonly bool[] _earlyReceived;              //for unordered

        private int _localSeqence;
        private int _remoteSequence;
        private int _localWindowStart;
        private int _remoteWindowStart;

        private readonly NetPeer _peer;
        private bool _mustSendAcks;

        private readonly bool _ordered;
        private readonly int _windowSize;
        private const int BitsInByte = 8;

        private int _queueIndex;

        public int PacketsInQueue
        {
            get { return _outgoingPackets.Count; }
        }

        public ReliableChannel(NetPeer peer, bool ordered, int windowSize)
        {
            _windowSize = windowSize;
            _peer = peer;
            _ordered = ordered;

            _outgoingPackets = new Queue<NetPacket>(_windowSize);

            _outgoingAcks = new bool[_windowSize];
            _pendingPackets = new PendingPacket[_windowSize];
            for (int i = 0; i < _pendingPackets.Length; i++)
            {
                _pendingPackets[i] = new PendingPacket();
            }

            if (_ordered)
                _receivedPackets = new NetPacket[_windowSize];
            else
                _earlyReceived = new bool[_windowSize];

            _localWindowStart = 0;
            _localSeqence = 0;
            _remoteSequence = 0;
            _remoteWindowStart = 0;
        }

        //ProcessAck in packet
        public void ProcessAck(NetPacket packet)
        {
            int validPacketSize = (_windowSize - 1) / BitsInByte + 1 + NetConstants.SequencedHeaderSize;
            if (packet.Size != validPacketSize)
            {
                NetUtils.DebugWrite("[PA]Invalid acks packet size");
                return;
            }

            ushort ackWindowStart = packet.Sequence;
            if (ackWindowStart > NetConstants.MaxSequence)
            {
                NetUtils.DebugWrite("[PA]Bad window start");
                return;
            }

            //check relevance
            if (NetUtils.RelativeSequenceNumber(ackWindowStart, _localWindowStart) <= -_windowSize)
            {
                NetUtils.DebugWrite("[PA]Old acks");
                return;
            }

            byte[] acksData = packet.RawData;
            NetUtils.DebugWrite("[PA]AcksStart: {0}", ackWindowStart);
            int startByte = NetConstants.SequencedHeaderSize;

            Monitor.Enter(_pendingPackets);
            for (int i = 0; i < _windowSize; i++)
            {
                int ackSequence = (ackWindowStart + i) % NetConstants.MaxSequence;
                if (NetUtils.RelativeSequenceNumber(ackSequence, _localWindowStart) < 0)
                {
                    //NetUtils.DebugWrite(ConsoleColor.Cyan, "[PA] SKIP OLD: " + ackSequence);
                    //Skip old ack
                    continue;
                }

                int currentByte = startByte + i / BitsInByte;
                int currentBit = i % BitsInByte;

                if ((acksData[currentByte] & (1 << currentBit)) == 0)
                {
                    //NetUtils.DebugWrite(ConsoleColor.Cyan, "[PA] SKIP FALSE: " + ackSequence);
                    //Skip false ack
                    continue;
                }

                if (ackSequence == _localWindowStart)
                {
                    //Move window
                    _localWindowStart = (_localWindowStart + 1) % NetConstants.MaxSequence;
                }

                NetPacket removed = _pendingPackets[ackSequence % _windowSize].GetAndClear();
                if (removed != null)
                {
                    _peer.Recycle(removed);

                    NetUtils.DebugWrite("[PA]Removing reliableInOrder ack: {0} - true", ackSequence);
                }
                else
                {
                    NetUtils.DebugWrite("[PA]Removing reliableInOrder ack: {0} - false", ackSequence);
                }
            }
            Monitor.Exit(_pendingPackets);
        }

        public void AddToQueue(NetPacket packet)
        {
            lock (_outgoingPackets)
            {
                _outgoingPackets.Enqueue(packet);
            }
        }

        private void ProcessQueuedPackets()
        {
            //get packets from queue
            while (_outgoingPackets.Count > 0)
            {
                int relate = NetUtils.RelativeSequenceNumber(_localSeqence, _localWindowStart);
                if (relate < _windowSize)
                {
                    NetPacket packet;
                    lock (_outgoingPackets)
                    {
                        packet = _outgoingPackets.Dequeue();
                    }
                    packet.Sequence = (ushort)_localSeqence;
                    _pendingPackets[_localSeqence % _windowSize].Packet = packet;
                    _localSeqence = (_localSeqence + 1) % NetConstants.MaxSequence;
                }
                else //Queue filled
                {
                    break;
                }
            }
        }

        public bool SendNextPacket()
        {
            //check sending acks
            DateTime currentTime = DateTime.UtcNow;

            Monitor.Enter(_pendingPackets);
            ProcessQueuedPackets();

            //send
            PendingPacket currentPacket;
            bool packetFound = false;
            int startQueueIndex = _queueIndex;
            do
            {
                currentPacket = _pendingPackets[_queueIndex];
                if (currentPacket.Packet != null)
                {
                    //check send time
                    if(currentPacket.TimeStamp.HasValue)
                    {
                        double packetHoldTime = (currentTime - currentPacket.TimeStamp.Value).TotalMilliseconds;
                        if (packetHoldTime > _peer.ResendDelay)
                        {
                            NetUtils.DebugWrite("[RC]Resend: {0} > {1}", (int)packetHoldTime, _peer.ResendDelay);
                            packetFound = true;
                        }
                    }
                    else //Never sended
                    {
                        packetFound = true;
                    }
                }

                _queueIndex = (_queueIndex + 1) % _windowSize;
            } while (!packetFound && _queueIndex != startQueueIndex);

            if (packetFound)
            {
                currentPacket.TimeStamp = DateTime.UtcNow;
                _peer.SendRawData(currentPacket.Packet);
                NetUtils.DebugWrite("[RR]Sended");
            }
            Monitor.Exit(_pendingPackets);
            return packetFound;
        }

        public void SendAcks()
        {
            if (!_mustSendAcks)
                return;
            _mustSendAcks = false;

            NetUtils.DebugWrite("[RR]SendAcks");

            //Init packet
            int bytesCount = (_windowSize - 1) / BitsInByte + 1;
            PacketProperty property = _ordered ? PacketProperty.AckReliableOrdered : PacketProperty.AckReliable;
            var acksPacket = _peer.GetPacketFromPool(property, bytesCount);

            //For quick access
            byte[] data = acksPacket.RawData; //window start + acks size

            //Put window start
            Monitor.Enter(_outgoingAcks);
            acksPacket.Sequence = (ushort)_remoteWindowStart;

            //Put acks
            int startAckIndex = _remoteWindowStart % _windowSize;
            int currentAckIndex = startAckIndex;
            int currentBit = 0;
            int currentByte = NetConstants.SequencedHeaderSize;
            do 
            {
                if (_outgoingAcks[currentAckIndex])
                {
                    data[currentByte] |= (byte)(1 << currentBit);
                }

                currentBit++;
                if (currentBit == BitsInByte)
                {
                    currentByte++;
                    currentBit = 0;
                }
                currentAckIndex = (currentAckIndex + 1) % _windowSize;
            } while (currentAckIndex != startAckIndex);
            Monitor.Exit(_outgoingAcks);

            _peer.SendRawData(acksPacket);
            _peer.Recycle(acksPacket);
        }

        //Process incoming packet
        public void ProcessPacket(NetPacket packet)
        {
            if (packet.Sequence >= NetConstants.MaxSequence)
            {
                NetUtils.DebugWrite("[RR]Bad sequence");
                return;
            }

            int relate = NetUtils.RelativeSequenceNumber(packet.Sequence, _remoteWindowStart);
            int relateSeq = NetUtils.RelativeSequenceNumber(packet.Sequence, _remoteSequence);

            if (relateSeq > _windowSize)
            {
                NetUtils.DebugWrite("[RR]Bad sequence");
                return;
            }

            //Drop bad packets
            if(relate < 0)
            {
                //Too old packet doesn't ack
                NetUtils.DebugWrite("[RR]ReliableInOrder too old");
                return;
            }
            if (relate >= _windowSize * 2)
            {
                //Some very new packet
                NetUtils.DebugWrite("[RR]ReliableInOrder too new");
                return;
            }

            //If very new - move window
            Monitor.Enter(_outgoingAcks);
            if (relate >= _windowSize)
            {
                //New window position
                int newWindowStart = (_remoteWindowStart + relate - _windowSize + 1) % NetConstants.MaxSequence;

                //Clean old data
                while (_remoteWindowStart != newWindowStart)
                {
                    _outgoingAcks[_remoteWindowStart % _windowSize] = false;
                    _remoteWindowStart = (_remoteWindowStart + 1) % NetConstants.MaxSequence;
                }
            }

            //Final stage - process valid packet
            //trigger acks send
            _mustSendAcks = true;

            if (_outgoingAcks[packet.Sequence % _windowSize])
            {
                NetUtils.DebugWrite("[RR]ReliableInOrder duplicate");
                Monitor.Exit(_outgoingAcks);
                return;
            }

            //save ack
            _outgoingAcks[packet.Sequence % _windowSize] = true;
            Monitor.Exit(_outgoingAcks);

            //detailed check
            if (packet.Sequence == _remoteSequence)
            {
                NetUtils.DebugWrite("[RR]ReliableInOrder packet succes");
                _peer.AddIncomingPacket(packet);
                _remoteSequence = (_remoteSequence + 1) % NetConstants.MaxSequence;

                if (_ordered)
                {
                    NetPacket p;
                    while ( (p = _receivedPackets[_remoteSequence % _windowSize]) != null)
                    {
                        //process holded packet
                        _receivedPackets[_remoteSequence % _windowSize] = null;
                        _peer.AddIncomingPacket(p);
                        _remoteSequence = (_remoteSequence + 1) % NetConstants.MaxSequence;
                    }
                }
                else
                {
                    while (_earlyReceived[_remoteSequence % _windowSize])
                    {
                        //process early packet
                        _earlyReceived[_remoteSequence % _windowSize] = false;
                        _remoteSequence = (_remoteSequence + 1) % NetConstants.MaxSequence;
                    }
                }

                return;
            }

            //holded packet
            if (_ordered)
            {
                _receivedPackets[packet.Sequence % _windowSize] = packet;
            }
            else
            {
                _earlyReceived[packet.Sequence % _windowSize] = true;
                _peer.AddIncomingPacket(packet);
            }
        }
    }
}
#endif
