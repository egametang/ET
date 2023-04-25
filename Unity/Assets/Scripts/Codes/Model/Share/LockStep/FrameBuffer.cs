using System;
using System.Collections.Generic;

namespace ET
{
    public class FrameBuffer
    {
        public int NowFrame { get; set; }

        public int RealFrame { get; set; } = -1;
        
        private const int TotalFrameCount = 128;
        
        private readonly List<OneFrameMessages> messageBuffer = new(TotalFrameCount);
        private readonly List<byte[]> dataBuffer = new(TotalFrameCount);

        public FrameBuffer()
        {
            for (int i = 0; i < this.dataBuffer.Capacity; ++i)
            {
                this.messageBuffer.Add(null);
                this.dataBuffer.Add(null);
            }
        }

        public void AddRealFrame(OneFrameMessages message)
        {
            if (message.Frame != this.RealFrame + 1)
            {
                throw new Exception($"add real frame error: {message.Frame} {this.RealFrame}");
            }
            this.RealFrame = message.Frame;
            AddFrame(message);
        }

        public void AddFrame(OneFrameMessages message)
        {
            this.messageBuffer[message.Frame % TotalFrameCount] = message;
        }
        
        public OneFrameMessages GetFrame(int frame)
        {
            if (frame < 0)
            {
                return null;
            }

            if (frame > this.RealFrame && frame > this.NowFrame)
            {
                return null;
            }
            return this.messageBuffer[frame % TotalFrameCount];
        }

        public void SaveDate(int frame, byte[] data)
        {
            this.dataBuffer[frame % TotalFrameCount] = data;
        }

        public byte[] GetDate(int frame)
        {
            return this.dataBuffer[frame % TotalFrameCount];
        }
    }
}