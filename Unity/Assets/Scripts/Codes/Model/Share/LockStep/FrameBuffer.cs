using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    public class FrameBuffer
    {
        public int PredictionFrame { get; set; }

        public int RealFrame { get; set; } = -1;
        
        private const int TotalFrameCount = 128;
        
        private readonly List<OneFrameMessages> messageBuffer = new(TotalFrameCount);
        private readonly List<MemoryBuffer> dataBuffer = new(TotalFrameCount);

        public FrameBuffer()
        {
            for (int i = 0; i < this.dataBuffer.Capacity; ++i)
            {
                this.messageBuffer.Add(new OneFrameMessages());
                this.dataBuffer.Add(new MemoryBuffer(10240));
            }
        }
        
        public OneFrameMessages GetFrame(int frame)
        {
            if (frame < 0)
            {
                return null;
            }
            OneFrameMessages oneFrameMessages = this.messageBuffer[frame % TotalFrameCount];
            oneFrameMessages.Frame = frame;
            return oneFrameMessages;
        }

        public LSWorld GetLSWorld(int frame)
        {
            MemoryBuffer memoryBuffer = this.dataBuffer[frame % TotalFrameCount];
            return MongoHelper.Deserialize(typeof (LSWorld), memoryBuffer) as LSWorld;
        }

        public void SaveLSWorld(int frame, LSWorld lsWorld)
        {
            MemoryBuffer memoryBuffer = this.dataBuffer[frame % TotalFrameCount];
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);
            MongoHelper.Serialize(lsWorld, memoryBuffer);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
        }
    }
}