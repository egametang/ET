using System.Threading;

namespace FlyingWormConsole3.LiteNetLib
{
    public sealed class NetStatistics 
    {
        private long _packetsSent;
        private long _packetsReceived;
        private long _bytesSent;
        private long _bytesReceived;
        private long _packetLoss;

        public long PacketsSent 
        {
            get { return Interlocked.Read(ref _packetsSent); }
        }

        public long PacketsReceived 
        {
            get { return Interlocked.Read(ref _packetsReceived); }
        }

        public long BytesSent 
        { 
            get { return Interlocked.Read(ref _bytesSent); }
        }
        public long BytesReceived 
        { 
            get { return Interlocked.Read(ref _bytesReceived); }
        }
        public long PacketLoss 
        { 
            get { return Interlocked.Read(ref _packetLoss); }
        }
        
        public long PacketLossPercent
        {
            get 
            {
                long sent = PacketsSent, loss = PacketLoss;
                
                return sent == 0 ? 0 : loss * 100 / sent;
            }
        }

        public void Reset() 
        {
            Interlocked.Exchange(ref _packetsSent, 0);
            Interlocked.Exchange(ref _packetsReceived, 0);
            Interlocked.Exchange(ref _bytesSent, 0);
            Interlocked.Exchange(ref _bytesReceived, 0);
            Interlocked.Exchange(ref _packetLoss, 0);
        }

        public void IncrementPacketsSent() 
        {
            Interlocked.Increment(ref _packetsSent);
        }

        public void IncrementPacketsReceived() 
        {
            Interlocked.Increment(ref _packetsReceived);
        }

        public void AddBytesSent(long bytesSent) 
        {
            Interlocked.Add(ref _bytesSent, bytesSent);
        }

        public void AddBytesReceived(long bytesReceived) 
        {
            Interlocked.Add(ref _bytesReceived, bytesReceived);
        }

        public void IncrementPacketLoss() 
        {
            Interlocked.Increment(ref _packetLoss);
        }

        public void AddPacketLoss(long packetLoss) 
        {
            Interlocked.Add(ref _packetLoss, packetLoss);
        }
        
        public override string ToString()
        {
            return
                string.Format(
                    "BytesReceived: {0}\nPacketsReceived: {1}\nBytesSent: {2}\nPacketsSent: {3}\nPacketLoss: {4}\nPacketLossPercent: {5}\n",
                    BytesReceived,
                    PacketsReceived,
                    BytesSent,
                    PacketsSent,
                    PacketLoss,
                    PacketLossPercent);
        }
    }
}
