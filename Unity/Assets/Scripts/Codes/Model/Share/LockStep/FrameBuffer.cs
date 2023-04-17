using System;
using System.Collections.Generic;

namespace ET
{
    public class FrameBuffer
    {
        private const int TotalFrameCount = 256;
        private int nowFrameCount;
        private readonly List<OneFrameMessages> messageBuffer = new(TotalFrameCount);
        private readonly List<byte[]> dataBuffer = new(TotalFrameCount);
        
        private void CheckFrame(int frame)
        {
            if (frame > this.nowFrameCount)
            {
                throw new Exception($"frame > max frame: {frame} {this.nowFrameCount}");
            }
            if (frame < this.nowFrameCount + 1 - TotalFrameCount || frame < 0)
            {
                throw new Exception($"frame < min frame: {frame} {this.nowFrameCount + 1 - TotalFrameCount}");
            }
        }

        public void AddFrameMessage(OneFrameMessages message)
        {
            this.messageBuffer[message.Frame % TotalFrameCount] = message;
            if (message.Frame > this.nowFrameCount)
            {
                this.nowFrameCount = message.Frame;
            }
        }
        
        public OneFrameMessages GetFrameMessage(int frame)
        {
            CheckFrame(frame);
            return this.messageBuffer[frame % TotalFrameCount];
        }

        public void SaveDate(int frame, byte[] data)
        {
            this.dataBuffer[frame % TotalFrameCount] = data;
        }

        public byte[] GetDate(int frame)
        {
            CheckFrame(frame);
            return this.dataBuffer[frame % TotalFrameCount];
        }
    }
}