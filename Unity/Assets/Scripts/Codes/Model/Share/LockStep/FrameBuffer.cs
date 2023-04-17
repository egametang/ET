using System;
using System.Collections.Generic;

namespace ET
{
    public class OneFrameMessage
    {
        private readonly SortedList<long, FrameMessage> messages = new (ConstValue.MatchCount);

        public SortedList<long, FrameMessage> Messages
        {
            get
            {
                return this.messages;
            }
        }
    }
    
    public class FrameBuffer
    {
        private const int TotalFrameCount = 256;
        private int NowFrameCount;
        private readonly List<OneFrameMessage> MessageBuffer = new(TotalFrameCount);
        private readonly List<byte[]> DataBuffer = new(TotalFrameCount);
        
        public FrameBuffer()
        {
            for (int i = 0; i < TotalFrameCount; i++)
            {
                this.MessageBuffer.Add(new OneFrameMessage());
            }
        }

        public void AddFrameMessage(FrameMessage message)
        {
            this.MessageBuffer[message.Frame % TotalFrameCount].Messages.Add(message.PlayerId, message);
            if (message.Frame > this.NowFrameCount)
            {
                this.NowFrameCount = message.Frame;
            }
        }
        
        public OneFrameMessage GetFrameMessage(int frame)
        {
            if (frame > this.NowFrameCount)
            {
                throw new Exception($"frame > max frame: {frame} {this.NowFrameCount}");
            }
            if (frame < this.NowFrameCount - TotalFrameCount || frame < 0)
            {
                throw new Exception($"frame < min frame: {frame} {this.NowFrameCount - 255}");
            }
            return this.MessageBuffer[frame % TotalFrameCount];
        }

        public void SaveDate(int frame, byte[] data)
        {
            this.DataBuffer[frame % TotalFrameCount] = data;
        }
    }
}