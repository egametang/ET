using System;
using System.Collections.Generic;

namespace ET
{
    public class FrameBuffer
    {
        public int NowFrame { get; set; }
        
        public int RealFrame { get; set; }
        
        public int PredictionCount { get; set; }
        
        private const int TotalFrameCount = 128;
        
        private readonly List<OneFrameMessages> messageBuffer = new(TotalFrameCount);
        private readonly List<byte[]> dataBuffer = new(TotalFrameCount);
        
        private bool CheckFrame(int frame)
        {
            if (frame > this.RealFrame)
            {
                return false;
            }
            if (frame < this.RealFrame + 1 - TotalFrameCount || frame < 0)
            {
                return false;
            }

            return true;
        }

        public void AddRealFrame(OneFrameMessages message)
        {
            if (message.Frame != this.RealFrame + 1)
            {
                throw new Exception($"add real frame error: {message.Frame} {this.RealFrame}");
            }
            this.RealFrame = message.Frame;
            this.messageBuffer[message.Frame % TotalFrameCount] = message;
        }
        
        public OneFrameMessages GetFrame(int frame)
        {
            if (!CheckFrame(frame))
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
            if (!CheckFrame(frame))
            {
                return null;
            }
            return this.dataBuffer[frame % TotalFrameCount];
        }
    }
}