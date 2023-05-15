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
        private readonly List<long> hashs;

        public FrameBuffer(int capacity = LSConstValue.FrameCountPerSecond * 10)
        {
            this.MaxFrame = capacity - 1;
            this.frameInputs = new List<OneFrameInputs>(capacity);
            this.snapshots = new List<MemoryBuffer>(capacity);
            this.hashs = new List<long>(capacity);
            
            for (int i = 0; i < this.snapshots.Capacity; ++i)
            {
                this.hashs.Add(0);
                this.frameInputs.Add(new OneFrameInputs());
                MemoryBuffer memoryBuffer = new(10240);
                memoryBuffer.SetLength(0);
                memoryBuffer.Seek(0, SeekOrigin.Begin);
                this.snapshots.Add(memoryBuffer);
            }
        }

        public void SetHash(int frame, long hash)
        {
            if (frame < 0)
            {
                return;
            }

            if (frame > this.MaxFrame)
            {
                return;
            }
            this.hashs[frame % this.frameInputs.Capacity] = hash;
        }
        
        public long GetHash(int frame)
        {
            if (frame < 0)
            {
                return 0;
            }

            if (frame > this.MaxFrame)
            {
                return 0;
            }
            return this.hashs[frame % this.frameInputs.Capacity];
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