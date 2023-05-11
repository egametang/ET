using System;
using System.Collections.Generic;
using System.IO;

namespace ET
{
    public class FrameBuffer
    {
        public int MaxFrame { get; private set; }
        private readonly List<OneFrameInputs> frameInputs;
        private readonly List<MemoryBuffer> snapshots;

        public FrameBuffer(int capacity = LSConstValue.FrameCountPerSecond * 10)
        {
            this.MaxFrame = capacity - 1;
            this.frameInputs = new List<OneFrameInputs>(capacity);
            this.snapshots = new List<MemoryBuffer>(capacity);
            
            for (int i = 0; i < this.snapshots.Capacity; ++i)
            {
                this.frameInputs.Add(new OneFrameInputs());
                MemoryBuffer memoryBuffer = new(10240);
                memoryBuffer.SetLength(0);
                memoryBuffer.Seek(0, SeekOrigin.Begin);
                this.snapshots.Add(memoryBuffer);
            }
        }
        
        public OneFrameInputs FrameInputs(int frame)
        {
            if (frame < 0)
            {
                return null;
            }

            if (frame > this.MaxFrame)
            {
                return null;
            }
            OneFrameInputs oneFrameInputs = this.frameInputs[frame % this.frameInputs.Capacity];
            return oneFrameInputs;
        }

        public void MoveForward(int frame)
        {
            if (this.MaxFrame - frame > LSConstValue.FrameCountPerSecond) // 至少留出1秒的空间
            {
                return;
            }
            
            ++this.MaxFrame;
            
            Log.Debug($"framebuffer move forward: {this.MaxFrame}");
            
            OneFrameInputs oneFrameInputs = this.FrameInputs(this.MaxFrame);
            oneFrameInputs.Inputs.Clear();
        }

        public MemoryBuffer Snapshot(int frame)
        {
            if (frame < 0)
            {
                return null;
            }

            if (frame > this.MaxFrame)
            {
                return null;
            }
            MemoryBuffer memoryBuffer = this.snapshots[frame % this.snapshots.Capacity];
            return memoryBuffer;
        }
    }
}