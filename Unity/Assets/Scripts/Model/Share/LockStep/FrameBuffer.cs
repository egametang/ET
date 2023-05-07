using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    public class FrameBuffer
    {
        public int MaxFrame { get; private set; }
        private readonly List<OneFrameMessages> messageBuffer;
        private readonly List<MemoryBuffer> dataBuffer;

        public FrameBuffer(int capacity = LSConstValue.FrameCountPerSecond * 10)
        {
            this.MaxFrame = capacity - 1;
            this.messageBuffer = new List<OneFrameMessages>(capacity);
            this.dataBuffer = new List<MemoryBuffer>(capacity);
            
            for (int i = 0; i < this.dataBuffer.Capacity; ++i)
            {
                this.messageBuffer.Add(new OneFrameMessages());
                MemoryBuffer memoryBuffer = new(10240);
                memoryBuffer.SetLength(0);
                memoryBuffer.Seek(0, SeekOrigin.Begin);
                this.dataBuffer.Add(memoryBuffer);
            }
        }
        
        public OneFrameMessages this[int frame]
        {
            get
            {
                if (frame < 0)
                {
                    return null;
                }

                if (frame > this.MaxFrame)
                {
                    return null;
                }
                OneFrameMessages oneFrameMessages = this.messageBuffer[frame % this.messageBuffer.Capacity];
                return oneFrameMessages;
            }
        }

        public void MoveForward(int frame)
        {
            if (this.MaxFrame - frame > LSConstValue.FrameCountPerSecond) // 至少留出1秒的空间
            {
                return;
            }
            
            ++this.MaxFrame;
            
            Log.Debug($"framebuffer move forward: {this.MaxFrame}");
            
            OneFrameMessages oneFrameMessages = this[this.MaxFrame];
            oneFrameMessages.Inputs.Clear();
            oneFrameMessages.Frame = 0;
        }

        public LSWorld GetLSWorld(int frame)
        {
            MemoryBuffer memoryBuffer = this.dataBuffer[frame % this.dataBuffer.Capacity];
            return MongoHelper.Deserialize(typeof (LSWorld), memoryBuffer) as LSWorld;
        }

        public void SaveLSWorld(int frame, LSWorld lsWorld)
        {
            MemoryBuffer memoryBuffer = this.dataBuffer[frame % this.dataBuffer.Capacity];
            memoryBuffer.Seek(0, SeekOrigin.Begin);
            memoryBuffer.SetLength(0);
            
            MongoHelper.Serialize(lsWorld, memoryBuffer);
            memoryBuffer.Seek(0, SeekOrigin.Begin);
        }
    }
}