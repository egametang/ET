using System;

namespace FlyingWormConsole3.LiteNetLib
{
    internal sealed class ReliableChannel : BaseChannel
    {
        private struct PendingPacket
        {
            private NetPacket _packet;
            private long _timeStamp;
            private bool _isSent;

            public override string ToString()
            {
                return _packet == null ? "Empty" : _packet.Sequence.ToString();
            }

            public void Init(NetPacket packet)
            {
                _packet = packet;
                _isSent = false;
            }

            public void TrySend(long currentTime, NetPeer peer, out bool hasPacket)
            {
                if (_packet == null)
                {
                    hasPacket = false;
                    return;
                }

                hasPacket = true;
                if (_isSent) //check send time
                {
                    double resendDelay = peer.ResendDelay * TimeSpan.TicksPerMillisecond;
                    double packetHoldTime = currentTime - _timeStamp;
                    if (packetHoldTime < resendDelay)
                        return;
                    NetDebug.Write("[RC]Resend: {0} > {1}", (int)packetHoldTime, resendDelay);
                }
                _timeStamp = currentTime;
                _isSent = true;
                peer.SendUserData(_packet);
            }

            public bool Clear(NetPeer peer)
            {
                if (_packet != null)
                {
                    peer.RecycleAndDeliver(_packet);
                    _packet = null;
                    return true;
                }
                return false;
            }
        }

        private readonly NetPacket _outgoingAcks;            //for send acks
        private readonly PendingPacket[] _pendingPackets;    //for unacked packets and duplicates
        private readonly NetPacket[] _receivedPackets;       //for order
        private readonly bool[] _earlyReceived;              //for unordered

        private int _localSeqence;
        private int _remoteSequence;
        private int _localWindowStart;
        private int _remoteWindowStart;

        private bool _mustSendAcks;

        private readonly DeliveryMethod _deliveryMethod;
        private readonly bool _ordered;
        private readonly int _windowSize;
        private const int BitsInByte = 8;
        private readonly byte _id;

        public ReliableChannel(NetPeer peer, bool ordered, byte id) : base(peer)
        {
            _id = id;
            _windowSize = NetConstants.DefaultWindowSize;
            _ordered = ordered;
            _pendingPackets = new PendingPacket[_windowSize];
            for (int i = 0; i < _pendingPackets.Length; i++)
                _pendingPackets[i] = new PendingPacket();

            if (_ordered)
            {
                _deliveryMethod = DeliveryMethod.ReliableOrdered;
                _receivedPackets = new NetPacket[_windowSize];
            }
            else
            {
                _deliveryMethod = DeliveryMethod.ReliableUnordered;
                _earlyReceived = new bool[_windowSize];
            }

            _localWindowStart = 0;
            _localSeqence = 0;
            _remoteSequence = 0;
            _remoteWindowStart = 0;
            _outgoingAcks = new NetPacket(PacketProperty.Ack, (_windowSize - 1) / BitsInByte + 2) {ChannelId = id};
        }

        //ProcessAck in packet
        private void ProcessAck(NetPacket packet)
        {
            if (packet.Size != _outgoingAcks.Size)
            {
                NetDebug.Write("[PA]Invalid acks packet size");
                return;
            }

            ushort ackWindowStart = packet.Sequence;
            int windowRel = NetUtils.RelativeSequenceNumber(_localWindowStart, ackWindowStart);
            if (ackWindowStart >= NetConstants.MaxSequence || windowRel < 0)
            {
                NetDebug.Write("[PA]Bad window start");
                return;
            }

            //check relevance     
            if (windowRel >= _windowSize)
            {
                NetDebug.Write("[PA]Old acks");
                return;
            }

            byte[] acksData = packet.RawData;
            lock (_pendingPackets)
            {
                for (int pendingSeq = _localWindowStart;
                    pendingSeq != _localSeqence;
                    pendingSeq = (pendingSeq + 1) % NetConstants.MaxSequence)
                {
                    int rel = NetUtils.RelativeSequenceNumber(pendingSeq, ackWindowStart);
                    if (rel >= _windowSize)
                    {
                        NetDebug.Write("[PA]REL: " + rel);
                        break;
                    }

                    int pendingIdx = pendingSeq % _windowSize;
                    int currentByte = NetConstants.ChanneledHeaderSize + pendingIdx / BitsInByte;
                    int currentBit = pendingIdx % BitsInByte;
                    if ((acksData[currentByte] & (1 << currentBit)) == 0)
                    {
                        if (Peer.NetManager.EnableStatistics) 
                        {
                            Peer.Statistics.IncrementPacketLoss();
                            Peer.NetManager.Statistics.IncrementPacketLoss();
                        }

                        //Skip false ack
                        NetDebug.Write("[PA]False ack: {0}", pendingSeq);
                        continue;
                    }

                    if (pendingSeq == _localWindowStart)
                    {
                        //Move window                
                        _localWindowStart = (_localWindowStart + 1) % NetConstants.MaxSequence;
                    }

                    //clear packet
                    if (_pendingPackets[pendingIdx].Clear(Peer))
                        NetDebug.Write("[PA]Removing reliableInOrder ack: {0} - true", pendingSeq);
                }
            }
        }

        protected override bool SendNextPackets()
        {
            if (_mustSendAcks)
            {
                _mustSendAcks = false;
                NetDebug.Write("[RR]SendAcks");
                lock(_outgoingAcks)
                    Peer.SendUserData(_outgoingAcks);
            }

            long currentTime = DateTime.UtcNow.Ticks;
            bool hasPendingPackets = false;

            lock (_pendingPackets)
            {
                //get packets from queue
                lock (OutgoingQueue)
                {
                    while (OutgoingQueue.Count > 0)
                    {
                        int relate = NetUtils.RelativeSequenceNumber(_localSeqence, _localWindowStart);
                        if (relate >= _windowSize)
                            break;

                        var netPacket = OutgoingQueue.Dequeue();
                        netPacket.Sequence = (ushort) _localSeqence;
                        netPacket.ChannelId = _id;
                        _pendingPackets[_localSeqence % _windowSize].Init(netPacket);
                        _localSeqence = (_localSeqence + 1) % NetConstants.MaxSequence;
                    }
                }

                //send
                for (int pendingSeq = _localWindowStart; pendingSeq != _localSeqence; pendingSeq = (pendingSeq + 1) % NetConstants.MaxSequence)
                {
                    // Please note: TrySend is invoked on a mutable struct, it's important to not extract it into a variable here
                    bool hasPacket;
                    _pendingPackets[pendingSeq % _windowSize].TrySend(currentTime, Peer, out hasPacket);
                    if (hasPacket)
                    {
                        hasPendingPackets = true;
                    }
                }
            }

            return hasPendingPackets || _mustSendAcks || OutgoingQueue.Count > 0;
        }

        //Process incoming packet
        public override bool ProcessPacket(NetPacket packet)
        {
            if (packet.Property == PacketProperty.Ack)
            {
                ProcessAck(packet);
                return false;
            }
            int seq = packet.Sequence;
            if (seq >= NetConstants.MaxSequence)
            {
                NetDebug.Write("[RR]Bad sequence");
                return false;
            }

            int relate = NetUtils.RelativeSequenceNumber(seq, _remoteWindowStart);
            int relateSeq = NetUtils.RelativeSequenceNumber(seq, _remoteSequence);

            if (relateSeq > _windowSize)
            {
                NetDebug.Write("[RR]Bad sequence");
                return false;
            }

            //Drop bad packets
            if (relate < 0)
            {
                //Too old packet doesn't ack
                NetDebug.Write("[RR]ReliableInOrder too old");
                return false;
            }
            if (relate >= _windowSize * 2)
            {
                //Some very new packet
                NetDebug.Write("[RR]ReliableInOrder too new");
                return false;
            }

            //If very new - move window
            int ackIdx;
            int ackByte;
            int ackBit;
            lock (_outgoingAcks)
            {
                if (relate >= _windowSize)
                {
                    //New window position
                    int newWindowStart = (_remoteWindowStart + relate - _windowSize + 1) % NetConstants.MaxSequence;
                    _outgoingAcks.Sequence = (ushort) newWindowStart;

                    //Clean old data
                    while (_remoteWindowStart != newWindowStart)
                    {
                        ackIdx = _remoteWindowStart % _windowSize;
                        ackByte = NetConstants.ChanneledHeaderSize + ackIdx / BitsInByte;
                        ackBit = ackIdx % BitsInByte;
                        _outgoingAcks.RawData[ackByte] &= (byte) ~(1 << ackBit);
                        _remoteWindowStart = (_remoteWindowStart + 1) % NetConstants.MaxSequence;
                    }
                }

                //Final stage - process valid packet
                //trigger acks send
                _mustSendAcks = true;
                
                ackIdx = seq % _windowSize;
                ackByte = NetConstants.ChanneledHeaderSize + ackIdx / BitsInByte;
                ackBit = ackIdx % BitsInByte;
                if ((_outgoingAcks.RawData[ackByte] & (1 << ackBit)) != 0)
                {
                    NetDebug.Write("[RR]ReliableInOrder duplicate");
                    return false;
                }

                //save ack
                _outgoingAcks.RawData[ackByte] |= (byte) (1 << ackBit);
            }

            AddToPeerChannelSendQueue();

            //detailed check
            if (seq == _remoteSequence)
            {
                NetDebug.Write("[RR]ReliableInOrder packet succes");
                Peer.AddReliablePacket(_deliveryMethod, packet);
                _remoteSequence = (_remoteSequence + 1) % NetConstants.MaxSequence;

                if (_ordered)
                {
                    NetPacket p;
                    while ((p = _receivedPackets[_remoteSequence % _windowSize]) != null)
                    {
                        //process holden packet
                        _receivedPackets[_remoteSequence % _windowSize] = null;
                        Peer.AddReliablePacket(_deliveryMethod, p);
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
                return true;
            }

            //holden packet
            if (_ordered)
            {
                _receivedPackets[ackIdx] = packet;
            }
            else
            {
                _earlyReceived[ackIdx] = true;
                Peer.AddReliablePacket(_deliveryMethod, packet);
            }
            return true;
        }
    }
}